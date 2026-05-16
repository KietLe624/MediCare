using AutoMapper;
using MediCare.API.Data;
using MediCare.API.DTOs;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Services
{
    public class PatientService : IPatientService
    {

        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientService> _logger;

        public PatientService(AppDbContext context, IMapper mapper, ILogger<PatientService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(PatientQueryParams query)
        {
            // lấy tất cả bệnh nhân
            var patientsQuery = _context.Patients.AsNoTracking();

            // filter(tên, UHID, SĐT)
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchLower = query.Search.ToLower(); // convert sang chữ thường
                patientsQuery = patientsQuery.Where(p =>
                    p.FirstName.ToLower().Contains(searchLower) ||
                    p.LastName.ToLower().Contains(searchLower) ||
                    p.UHID.ToLower().Contains(searchLower) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchLower))
                );
            }

            // filter loại bệnh nhân (inpatient, outpatient)
            if (!string.IsNullOrEmpty(query.PatientType))
            {
                patientsQuery = patientsQuery.Where(p => p.PatientType == query.PatientType);
            }

            // filter nhóm máu
            if (!string.IsNullOrEmpty(query.BloodType))
            {
                patientsQuery = patientsQuery.Where(p => p.BloodType == query.BloodType);
            }

            var totalCount = await patientsQuery.CountAsync();

            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);
            var patients = await patientsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<PatientSummaryResponse>>(patients); // map sang DTO trả về

            return new PagedResponse<PatientSummaryResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<PatientResponse> CreateAsync(CreatePatientRequest request, long createdByUserId)
        {
            if (request.UserId.HasValue)
            {
                // kiểm tra nếu có UserId thì phải tồn tại user đó
                var userExists = _context.Users.Any(u => u.Id == request.UserId.Value);
                if (!userExists)
                {
                    throw new KeyNotFoundException($"Không tìm thấy user với ID {request.UserId.Value}");
                }
                var userAlreadyPatient = _context.Patients.Any(p => p.UserId == request.UserId.Value);
                if (userAlreadyPatient)
                {
                    throw new InvalidOperationException($"User với ID {request.UserId.Value} đã liên kết với một hồ sơ khác");
                }
                
            }
            var patient = _mapper.Map<Patient>(request);
            patient.UHID = GenerateUHIDAsync().GetAwaiter().GetResult(); // sinh UHID
            patient.CreatedBy = createdByUserId;
            patient.CreatedAt = DateTime.UtcNow;
            _context.Patients.Add(patient); // thêm vào DbContext
            _context.SaveChanges(); // lưu vào database

            _logger.LogInformation(
            "Tạo thành công hồ sơ bệnh nhân: UHID={UHID}, Id={Id}, By={UserId}",
            patient.UHID, patient.Id, createdByUserId);

            return Task.FromResult(_mapper.Map<PatientResponse>(patient));
        }

        public async Task<PatientResponse> GetByIdAsync(long id)
        {
            var patient = await FindPatientOrThrowAsync(id);

            // sử dụng AutoMapper để map từ entity sang DTO trả về
            return _mapper.Map<PatientResponse>(patient);
        }

        public async Task<PatientResponse> UpdateAsync(long id, UpdatePatientRequest request, long updatedByUserId)
        {
            // tìm bệnh nhân, nếu không có sẽ ném lỗi
            var patient = await FindPatientOrThrowAsync(id);
            _mapper.Map(request, patient);
            patient.UpdatedBy = updatedByUserId;
            patient.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Patient updated: Id={Id}, By={UserId}", id, updatedByUserId);

            return _mapper.Map<PatientResponse>(patient);
        }

        // GET ALL HISTORY (APPOINTMENT, VISIT, PRESCRIPTION, INVOICE)
        public async Task<PagedResponse<PatientAppointmentResponse>> GetAppointmentsAsync(long patientId, PatientHistoryQueryParams query)
        {
            var patient = await FindPatientOrThrowAsync(patientId);

            // lấy thông tin lịch sử khám bệnh của bệnh nhân từ bảng Appointments
            var appointmentsQuery = _context.Appointments
                .Where(a => a.PatientId == patientId)
                .AsNoTracking();

            // Include Doctor và Department để lấy thông tin hiển thị
            var q = _context.Appointments
                .AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)           // lấy FullName bác sĩ
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)     // lấy tên khoa
                .OrderByDescending(a => a.AppointmentDate);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var appointments = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = appointments.Select(a => new PatientAppointmentResponse
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate.ToDateTime(TimeOnly.MinValue),
                StartTime = a.StartTime.ToString(@"hh\:mm"),
                EndTime = a.EndTime.ToString(@"hh\:mm"),
                Status = a.Status,
                Reason = a.Reason,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                Doctor = new DoctorBriefResponse
                {
                    Id = a.Doctor.Id,
                    FullName = a.Doctor.User.FullName,
                    Specialization = a.Doctor.Specialization ?? "",
                },
                Department = new DepartmentBriefResponse
                {
                    Id = a.Doctor.Department.Id,
                    Name = a.Doctor.Department.Name
                }
            }).ToList();

            return new PagedResponse<PatientAppointmentResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

        }

        public async Task<PagedResponse<PatientVisitResponse>> GetVisitsAsync(
            long patientId, PatientHistoryQueryParams query)
        {
            await EnsurePatientExistsAsync(patientId);

            var q = _context.Visits
                .AsNoTracking()
                .Where(v => v.Appointment.PatientId == patientId)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(v => v.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.Department)
                .OrderByDescending(v => v.VisitDate);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var visits = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Đếm số đơn thuốc trong từng lần khám
            var visitIds = visits.Select(v => v.Id).ToList();
            var prescriptionCounts = await _context.Prescriptions
                .Where(p => visitIds.Contains(p.VisitId))
                .GroupBy(p => p.VisitId)
                .Select(g => new { VisitId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.VisitId, x => x.Count);

            var items = visits.Select(v => new PatientVisitResponse
            {
                Id = v.Id,
                VisitDate = v.VisitDate,
                Symptoms = v.Symptoms,
                Diagnosis = v.Diagnosis,
                Treatment = v.Treatment,
                Notes = v.Notes,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
                Doctor = new DoctorBriefResponse
                {
                    Id = v.Appointment.Doctor.Id,
                    FullName = v.Appointment.Doctor.User.FullName,
                    Specialization = v.Appointment.Doctor.Specialization ?? "",
                },
                // Lấy từ dictionary đã query batch — tránh N+1
                PrescriptionCount = prescriptionCounts.GetValueOrDefault(v.Id, 0)
            }).ToList();

            return new PagedResponse<PatientVisitResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─── LỊCH SỬ ĐƠN THUỐC ───────────────────────────────────
        public async Task<PagedResponse<PatientPrescriptionResponse>> GetPrescriptionsAsync(
            long patientId, PatientHistoryQueryParams query)
        {
            await EnsurePatientExistsAsync(patientId);

            var q = _context.Prescriptions
                .AsNoTracking()
                .Where(p => p.Visit.Appointment.PatientId == patientId)
                .Include(p => p.Medication)
                .Include(p => p.Visit)
                    .ThenInclude(v => v.Appointment)
                        .ThenInclude(a => a.Doctor)
                            .ThenInclude(d => d.User)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var prescriptions = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = prescriptions.Select(p => new PatientPrescriptionResponse
            {
                Id = p.Id,
                CreatedAt = p.CreatedAt,
                Dosage = p.Dosage,
                Frequency = p.Frequency,
                Duration = p.Duration,
                Instructions = p.Instructions,
                VisitId = p.VisitId,
                VisitDate = p.Visit.VisitDate,
                //Medication = new MedicationBriefResponse
                //{
                //    Id = p.Medication.Id,
                //    Name = p.Medication.Name,
                //    GenericName = p.Medication.GenericName,
                //    Unit = p.Medication.Unit
                //},
                Doctor = new DoctorBriefResponse
                {
                    Id = p.Visit.Appointment.Doctor.Id,
                    FullName = p.Visit.Appointment.Doctor.User.FullName,
                    Specialization = p.Visit.Appointment.Doctor.Specialization ?? "",
                }
            }).ToList();

            return new PagedResponse<PatientPrescriptionResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─── LỊCH SỬ HÓA ĐƠN ────────────────────────────────────
        public async Task<PagedResponse<PatientInvoiceResponse>> GetInvoicesAsync(
            long patientId, PatientHistoryQueryParams query)
        {
            await EnsurePatientExistsAsync(patientId);

            var q = _context.Invoices
                .AsNoTracking()
                .Where(i => i.PatientId == patientId)
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.CreatedAt);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var invoices = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = invoices.Select(i => new PatientInvoiceResponse
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Status = i.Status,
                PaymentMethod = i.PaymentMethod,
                SubTotal = i.SubTotal,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Notes = i.Notes,
                IssuedAt = i.IssuedAt,
                PaidAt = i.PaidAt,
                CreatedAt = i.CreatedAt,
                //Items = i.Items.Select(item => new InvoiceItemBriefResponse
                //{
                //    Id = item.Id,
                //    ItemType = item.ItemType,
                //    Description = item.Description,
                //    Quantity = item.Quantity,
                //    UnitPrice = item.UnitPrice,
                //    TotalPrice = item.TotalPrice
                //}).ToList()
            }).ToList();

            return new PagedResponse<PatientInvoiceResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        // HELPER
        private async Task EnsurePatientExistsAsync(long patientId)
        {
            var exists = await _context.Patients.AnyAsync(p => p.Id == patientId);
            if (!exists)
                throw new KeyNotFoundException($"Không tìm thấy bệnh nhân với Id = {patientId}");
        }
        private async Task<string> GenerateUHIDAsync()
        {
            /// <summary>
            /// Sinh UHID: PT-YYYYMMDD-{4 số thứ tự trong ngày}
            /// Ví dụ: PT-20250410-0001
            /// </summary>
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"PT-{today}-";

            // Đếm số bệnh nhân được tạo trong ngày để sinh số thứ tự
            var countToday = await _context.Patients
                .CountAsync(p => p.UHID.StartsWith(prefix));

            return $"{prefix}{(countToday + 1):D4}";
        }

        private async Task<Patient> FindPatientOrThrowAsync(long id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy bệnh nhân với ID {id}");
            }
            return patient;
        }

        // MAPPING
        private static PatientAppointmentResponse MapToResponse(Appointment a)
            => new PatientAppointmentResponse   
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate.ToDateTime(TimeOnly.MinValue),
                StartTime = a.StartTime.ToString(@"hh\:mm"),
                EndTime = a.EndTime.ToString(@"hh\:mm"),
                Status = a.Status,
                Reason = a.Reason,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                Doctor = new DoctorBriefResponse
                {
                    Id = a.Doctor.Id,
                    FullName = a.Doctor.User.FullName,
                    Specialization = a.Doctor.Specialization ?? "",
                },
            };
        }
     
    
}

