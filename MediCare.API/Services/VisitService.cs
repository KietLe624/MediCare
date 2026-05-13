using MediCare.API.Data;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.DoctorDTO;
using static MediCare.API.DTOs.VisitDTO;


namespace MediCare.API.Services
{
    public class VisitService : IVisitService
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;

        public VisitService(AppDbContext context, ILogger<VisitService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // VISITS
        public async Task<PagedResponse<VisitSummaryResponse>> GetAllAsync(VisitQueryParams query)
        {
            /*
             * Lấy thông tin visit kèm theo appointment, patient, doctor, department
             * Visit -> Appointment -> Patient
             */
            var q = _context.Visits
                .AsNoTracking()
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.Department)
                .AsQueryable();

            // FILTER
            if (query.PatientId.HasValue)
                q = q.Where(v => v.Appointment.PatientId == query.PatientId.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(v => v.Appointment.DoctorId == query.DoctorId.Value);

            if (!string.IsNullOrWhiteSpace(query.Status))
                q = q.Where(v => v.Status == query.Status);

            if (query.FromDate.HasValue)
                q = q.Where(v => DateOnly.FromDateTime(v.VisitDate) >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                q = q.Where(v => DateOnly.FromDateTime(v.VisitDate) <= query.ToDate.Value);


            // SORT
            q = query.SortOrder == "asc"
               ? q.OrderBy(v => v.VisitDate)
               : q.OrderByDescending(v => v.VisitDate);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var visits = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Đếm số đơn thuốc theo từng visit — 1 query batch, tránh N+1
            var visitIds = visits.Select(v => v.Id).ToList();
            var prescriptionCounts = await _context.Prescriptions
                .Where(p => visitIds.Contains(p.VisitId))
                .GroupBy(p => p.VisitId)
                .Select(g => new { VisitId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.VisitId, x => x.Count);

            var items = visits.Select(v => new VisitSummaryResponse
            {
                Id = v.Id,
                AppointmentId = v.AppointmentId,
                VisitDate = v.VisitDate,
                Diagnosis = v.Diagnosis,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                PrescriptionCount = prescriptionCounts.GetValueOrDefault(v.Id, 0),
                Patient = new DTOs.VisitDTO.PatientBriefResponse // tránh lặp lại nhiều thông tin của patient trong appointment
                {
                    Id = v.Appointment.Patient.Id,
                    UHID = v.Appointment.Patient.UHID,
                    FullName = $"{v.Appointment.Patient.FirstName} {v.Appointment.Patient.LastName}",
                    UserId = v.Appointment.Patient.UserId
                },
                Doctor = new DTOs.VisitDTO.DoctorBriefResponse
                {
                    Id = v.Appointment.Doctor.Id,
                    FullName = v.Appointment.Doctor.User.FullName,
                    Specialization = v.Appointment.Doctor.Specialization,
                    Department = v.Appointment.Doctor.Department.Name
                }
            }).ToList();

            return new PagedResponse<VisitSummaryResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<VisitResponse> GetByIdAsync(long visitId)
        {
            var visit = await FindVisitOrThrowAsync(visitId);
            return MapToVisitResponse(visit);
        }

        public async Task<VisitResponse> CreateAsync(
           CreateVisitRequest request, long createdByUserId)
        {
            // Kiểm tra Appointment tồn tại và đang Confirmed
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId);

            if (appointment == null)
                throw new BadHttpRequestException(
                    $"Không tìm thấy lịch hẹn với ID {request.AppointmentId}");

            // Chỉ tạo Visit từ appointment đã Confirmed hoặc Scheduled
            if (appointment.Status != "Confirmed" && appointment.Status != "Scheduled")
                throw new BadHttpRequestException(
                    $"Không thể bắt đầu khám với lịch hẹn đang ở trạng thái '{appointment.Status}'");

            // Mỗi appointment chỉ có đúng 1 visit
            var visitExists = await _context.Visits
                .AnyAsync(v => v.AppointmentId == request.AppointmentId);
            if (visitExists)
                throw new BadHttpRequestException(
                    "Lịch hẹn này đã có bệnh án. Không thể tạo thêm.");

            if (appointment.AppointmentDate >
                DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadHttpRequestException(
                    "Không thể bắt đầu khám trước ngày hẹn."); 

            var visit = new Visit
            {
                AppointmentId = request.AppointmentId,
                VisitDate = request.VisitDate ?? DateTime.UtcNow, // mặc định là lúc bắt đầu khám
                Symptoms = request.Symptoms,
                Diagnosis = request.Diagnosis,
                Treatment = request.Treatment,
                Notes = request.Notes,
                Status = "InProgress",
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Visits.Add(visit);

            // Tự động cập nhật Appointment → Confirmed khi bắt đầu khám
            if (appointment.Status == "Scheduled")
            {
                appointment.Status = "Confirmed";
                appointment.UpdatedAt = DateTime.UtcNow;
                appointment.UpdatedBy = createdByUserId;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Visit created: Id={Id}, AppointmentId={AppointmentId}, By={UserId}",
                visit.Id, visit.AppointmentId, createdByUserId);

            return await GetByIdAsync(visit.Id);
        }

        // CHANGE STATUS
        public async Task<VisitResponse> CompleteAsync(long visitId, long updatedByUserId)
        {
            var visit = await FindVisitOrThrowAsync(visitId);
            // Chỉ cho phép chuyển sang Completed khi đang InProgress
            if (visit.Status != "InProgress")
                throw new BadHttpRequestException(
                    $"Chỉ có thể hoàn thành bệnh án đang ở trạng thái 'InProgress'. Hiện tại: '{visit.Status}'");

            visit.Status = "Completed";
            visit.UpdatedAt = DateTime.UtcNow;
            visit.UpdatedBy = updatedByUserId;

            // Chuyển appointment sang Completed 
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == visit.AppointmentId);

            if (appointment != null)
            {
                appointment.Status = "Completed";
                appointment.UpdatedAt = DateTime.UtcNow;
                appointment.UpdatedBy = updatedByUserId;
            }

            // Lưu thay đổi
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Visit completed: Id={Id}, By={UserId}",
                visit.Id, updatedByUserId);

            // Trả về thông tin visit đã cập nhật
            return MapToVisitResponse(visit);
        }
        public async Task<VisitResponse> CancelAsync(long visitId, long updatedByUserId)
        {
            var visit = FindVisitOrThrowAsync(visitId).Result;

            // Chỉ cho phép hủy khi visit đang InProgress
            if (visit.Status != "InProgress")
                throw new BadHttpRequestException(
                    $"Chỉ có thể hủy bệnh án đang ở trạng thái 'InProgress'. Hiện tại: '{visit.Status}'");

            visit.Status = "Cancelled";
            visit.UpdatedAt = DateTime.UtcNow;
            visit.UpdatedBy = updatedByUserId;

            // Lưu thay đổi
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Visit cancelled: Id={Id}, By={UserId}",
                visit.Id, updatedByUserId);

            return MapToVisitResponse(visit);
        }
        public async Task<VisitResponse> UpdateAsync(long visitId, UpdateVisitRequest request, long updatedByUserId)
        {
            var visit = await FindVisitOrThrowAsync(visitId);

            // Chỉ cho phép cập nhật khi visit đang InProgress
            if(visit.Status != "InProgress")
                throw new BadHttpRequestException(
                    $"Chỉ có thể cập nhật bệnh án đang ở trạng thái 'InProgress'. Hiện tại: '{visit.Status}'");

            // Cập nhật thông tin bệnh án
            visit.Symptoms = request.Symptoms;
            visit.Diagnosis = request.Diagnosis;
            visit.Treatment = request.Treatment;
            visit.Notes = request.Notes;
            visit.UpdatedAt = DateTime.UtcNow;
            visit.UpdatedBy = updatedByUserId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Visit updated: Id={Id}, By={UserId}",
                visit.Id, updatedByUserId);

            return MapToVisitResponse(visit);
        }

        // PRESCRIPTIONS
        public async Task<PrescriptionResponse> AddPrescriptionAsync(long visitId, AddPrescriptionRequest request, long createdByUserId)
        {
            var visit = await FindVisit(visitId);

            // Chỉ kê đơn khi đang khám
            if (visit.Status != "InProgress")
                throw new BadHttpRequestException(
                    "Chỉ có thể kê đơn thuốc khi bệnh án đang InProgress");

            // Kiểm tra Medication tồn tại và đang active
            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.Id == request.MedicationId && m.IsActive);
            if (medication == null)
                throw new BadHttpRequestException(
                    $"Không tìm thấy thuốc với ID {request.MedicationId}");

            var prescription = new Prescription
            {
                VisitId = visit.Id,
                MedicationId = medication.Id,
                Dosage = request.Dosage,
                Frequency = request.Frequency,
                Duration = request.Duration,
                Instructions = request.Instructions,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Prescription added: Id={Id}, VisitId={VisitId}, By={UserId}",
                prescription.Id, visit.Id, createdByUserId);

            return MapToPrescriptionResponse(prescription);
        }

        public async Task<PrescriptionResponse> UpdatePrescriptionAsync(long visitId, long prescriptionId, UpdatePrescriptionRequest request, long updatedByUserId)
        {
            var prescription = await FindPrescriptionOrThrowAsync(visitId, prescriptionId);

            var visit = await FindVisit(prescription.VisitId);
            
            if (visit?.Status != "InProgress")
                throw new BadHttpRequestException(
                    "Chỉ có thể sửa đơn thuốc khi bệnh án đang InProgress");

            // Kiểm tra Medication mới tồn tại
            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.Id == request.MedicationId && m.IsActive);

            if (medication == null)
                throw new BadHttpRequestException(
                    $"Không tìm thấy thuốc với ID {request.MedicationId}");

            prescription.Dosage = request.Dosage;
            prescription.Frequency = request.Frequency;
            prescription.Duration = request.Duration;
            prescription.Instructions = request.Instructions;
            prescription.UpdatedAt = DateTime.UtcNow;
            prescription.UpdatedBy = updatedByUserId;
            
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Prescription updated: Id={Id}, VisitId={VisitId}, By={UserId}",
                prescription.Id, visit.Id, updatedByUserId);
            return MapToPrescriptionResponse(prescription);
        }

        public async Task DeletePrescriptionAsync(long visitId, long prescriptionId)
        {
            var prescription = await FindPrescriptionOrThrowAsync(visitId, prescriptionId);

            // Kiểm tra trạng thái visit
            var visit = await FindVisit(prescription.VisitId);
            if (visit?.Status != "InProgress")
                throw new BadHttpRequestException(
                    "Chỉ có thể xóa đơn thuốc khi bệnh án đang InProgress");

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Prescription deleted: Id={Id}, VisitId={VisitId}",
                prescription.Id, visit.Id);
        }


        // HELPER 
        private async Task<Visit> FindVisitOrThrowAsync(long id)
        {
            var visit = await _context.Visits
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.Department)
                .Include(v => v.Prescriptions)
                    .ThenInclude(p => p.Medication)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null)
                throw new KeyNotFoundException($"Không tìm thấy bệnh án với ID {id}");

            return visit;
        }

        private async Task<Visit> FindVisit(long id)
        {
            var visit = await _context.Visits
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visit == null)
                throw new KeyNotFoundException($"Không tìm thấy bệnh án với ID {id}");

            return visit;
        }
        private async Task<Prescription> FindPrescriptionOrThrowAsync(
            long visitId, long prescriptionId)
        {
            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.VisitId == visitId);

            if (prescription == null)
                throw new KeyNotFoundException(
                    $"Không tìm thấy đơn thuốc ID {prescriptionId} trong bệnh án ID {visitId}");

            return prescription;
        }

        // MAPPING
        private static VisitResponse MapToVisitResponse(Visit v) => new()
        {
            Id = v.Id,
            AppointmentId = v.AppointmentId,
            VisitDate = v.VisitDate,
            Symptoms = v.Symptoms,
            Diagnosis = v.Diagnosis,
            Treatment = v.Treatment,
            Notes = v.Notes,
            Status = v.Status,
            CreatedAt = v.CreatedAt,
            Patient = new DTOs.VisitDTO.PatientBriefResponse
            {
                Id = v.Appointment.Patient.Id,
                UHID = v.Appointment.Patient.UHID,
                FullName = $"{v.Appointment.Patient.FirstName} {v.Appointment.Patient.LastName}",
                UserId = v.Appointment.Patient.UserId
            },
            Doctor = new DoctorBriefResponse
            {
                Id = v.Appointment.Doctor.Id,
                FullName = v.Appointment.Doctor.User.FullName,
                Specialization = v.Appointment.Doctor.Specialization,
                Department = v.Appointment.Doctor.Department.Name
            },
            Prescriptions = v.Prescriptions
                .OrderBy(p => p.CreatedAt)
                .Select(MapToPrescriptionResponse)
                .ToList()
        };
        private static PrescriptionResponse MapToPrescriptionResponse(Prescription p) => new()
        {
            Id = p.Id,
            VisitId = p.VisitId,
            Dosage = p.Dosage,
            Frequency = p.Frequency,
            Duration = p.Duration,
            Instructions = p.Instructions,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            Medication = new MedicationBriefResponse
            {
                Id = p.Medication.Id,
                Name = p.Medication.Name,
                GenericName = p.Medication.GenericName,
                Unit = p.Medication.Unit,
                Category = p.Medication.Category
            }
        };


    }
}
