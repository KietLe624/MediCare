using MediCare.API.Data;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.AppointmentDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Services
{
    public class AppointmentService : IAppointmentService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(AppDbContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResponse<AppointmentSummaryResponse>> GetAllAsync(AppointmentQueryParams query)
        {
            var q = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .AsQueryable();

            // filter
            if (query.Date.HasValue)
                q = q.Where(a => a.AppointmentDate == query.Date.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(a => a.DoctorId == query.DoctorId.Value);

            if (query.PatientId.HasValue)
                q = q.Where(a => a.PatientId == query.PatientId.Value);

            if (!string.IsNullOrWhiteSpace(query.Status))
                q = q.Where(a => a.Status == query.Status);

            // sort

            q = query.SortOrder == "asc"
                ? q.OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
                : q.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var appointments = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<AppointmentSummaryResponse>
            {
                Data = appointments.Select(MapToSummaryResponse).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

        }
        public async Task<AppointmentResponse> GetByIdAsync(long appointmentId)
        {
            var appointment = await FindAppointmentOrThrowAsync(appointmentId);
            return MapToResponse(appointment);
        }
        public async Task<AppointmentResponse> CreateAsync(
            CreateAppointmentRequest request, long createdByUserId)
        {
            // Validate dữ liệu đầu vào
            if (request.EndTime <= request.StartTime)
                throw new BadHttpRequestException("Giờ kết thúc phải sau giờ bắt đầu");

            if (request.AppointmentDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadHttpRequestException("Không thể đặt lịch,vui lòng kiểm tra lại ngày đặt lịch");

            // Kiểm tra Patient tồn tại
            var patientExists = await _context.Patients
                .AnyAsync(p => p.Id == request.PatientId);
            if (!patientExists)
                throw new BadHttpRequestException(
                    $"Không tìm thấy bệnh nhân với ID {request.PatientId}");

            // Kiểm tra Doctor tồn tại và đang available
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId);
            if (doctor == null)
                throw new BadHttpRequestException(
                    $"Không tìm thấy bác sĩ với ID {request.DoctorId}");
            if (!doctor.IsAvailable)
                throw new BadHttpRequestException("Bác sĩ hiện không nhận lịch hẹn");

            // Kiểm tra bác sĩ có làm việc vào ngày và khung giờ đó không
            var dayOfWeek = (int)request.AppointmentDate.DayOfWeek == 0
                ? 7 : (int)request.AppointmentDate.DayOfWeek;

            var hasSchedule = await _context.DoctorSchedules.AnyAsync(s =>
                s.DoctorId == request.DoctorId &&
                s.DayOfWeek == dayOfWeek &&
                s.IsActive &&
                s.StartTime <= request.StartTime &&
                s.EndTime >= request.EndTime);

            if (!hasSchedule)
                throw new BadHttpRequestException(
                    "Bác sĩ không có lịch làm việc vào khung giờ này");

            // Kiểm tra double-booking — bác sĩ đã có appointment cùng ngày + giờ chưa
            var isDoubleBooked = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == request.DoctorId &&
                a.AppointmentDate == request.AppointmentDate &&
                a.StartTime == request.StartTime &&
                a.Status != "Cancelled" &&
                a.Status != "NoShow");

            if (isDoubleBooked)
                throw new BadHttpRequestException(
                    "Khung giờ này đã được đặt. Vui lòng chọn giờ khác.");

            var appointment = new Appointment
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = "Scheduled",
                Reason = request.Reason,
                Notes = request.Notes,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Appointment created: Id={Id}, Patient={PatientId}, Doctor={DoctorId}, Date={Date}",
                appointment.Id, appointment.PatientId,
                appointment.DoctorId, appointment.AppointmentDate);

            return await GetByIdAsync(appointment.Id);
        }

        public async Task<AppointmentResponse> UpdateAsync(
            long appointmentId, UpdateAppointmentRequest request, long updatedByUserId)
        {
            var appointment = await FindAppointmentOrThrowAsync(appointmentId);

            // Chỉ cho phép sửa khi còn Scheduled hoặc Confirmed
            EnsureAppointmentIsModifiable(appointment);

            if (request.EndTime <= request.StartTime)
                throw new BadHttpRequestException("Giờ kết thúc phải sau giờ bắt đầu");

            if (request.AppointmentDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadHttpRequestException("Không thể đặt lịch trong quá khứ");

            // Kiểm tra double-booking (loại trừ chính nó)
            var isDoubleBooked = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == request.AppointmentDate &&
                a.StartTime == request.StartTime &&
                a.Id != appointmentId &&
                a.Status != "Cancelled" &&
                a.Status != "NoShow");

            if (isDoubleBooked)
                throw new BadHttpRequestException(
                    "Khung giờ này đã được đặt. Vui lòng chọn giờ khác.");

            appointment.AppointmentDate = request.AppointmentDate;
            appointment.StartTime = request.StartTime;
            appointment.EndTime = request.EndTime;
            appointment.Reason = request.Reason;
            appointment.Notes = request.Notes;
            appointment.UpdatedBy = updatedByUserId;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment updated: Id={Id}", appointmentId);

            return await GetByIdAsync(appointmentId);
        }

        public async Task<AppointmentResponse> ConfirmAsync(long id, long updatedByUserId)
        {
            var appointment = await FindAppointmentOrThrowAsync(id);

            // Scheduled → Confirmed
            if (appointment.Status != "Scheduled")
                throw new BadHttpRequestException(
                    $"Không thể xác nhận lịch hẹn đang ở trạng thái '{appointment.Status}'");

            return await ChangeStatusAsync(appointment, "Confirmed", updatedByUserId);
        }

        public async Task<AppointmentResponse> CompleteAsync(long id, long updatedByUserId)
        {
            var appointment = await FindAppointmentOrThrowAsync(id);

            // Confirmed → Completed
            if (appointment.Status != "Confirmed")
                throw new BadHttpRequestException(
                    $"Không thể hoàn thành lịch hẹn đang ở trạng thái '{appointment.Status}'");

            return await ChangeStatusAsync(appointment, "Completed", updatedByUserId);
        }

        public async Task<AppointmentResponse> CancelAsync(
            long id, CancelAppointmentRequest request, long updatedByUserId)
        {
            var appointment = await FindAppointmentOrThrowAsync(id);

            // Không hủy thể nếu đã hoàn thành hoặc đã hủy rồi
            if (appointment.Status == "Completed" || appointment.Status == "Cancelled")
                throw new BadHttpRequestException(
                    $"Không thể hủy lịch hẹn đang ở trạng thái '{appointment.Status}'");

            // Ghi lý do hủy vào Notes
            if (!string.IsNullOrWhiteSpace(request.Reason))
                appointment.Notes = $"[Hủy] {request.Reason}";

            return await ChangeStatusAsync(appointment, "Cancelled", updatedByUserId);
        }

        public async Task<AppointmentResponse> NoShowAsync(long id, long updatedByUserId)
        {
            var appointment = await FindAppointmentOrThrowAsync(id);

            if (appointment.Status != "Scheduled" && appointment.Status != "Confirmed")
                throw new BadHttpRequestException(
                    $"Không thể đánh dấu NoShow lịch hẹn ở trạng thái '{appointment.Status}'");

            return await ChangeStatusAsync(appointment, "NoShow", updatedByUserId);
        }

        public async Task<AppointmentResponse> RescheduleAsync(
            long id, RescheduleAppointmentRequest request, long updatedByUserId)
        {
            var appointment = await FindAppointmentOrThrowAsync(id);

            EnsureAppointmentIsModifiable(appointment);

            if (request.NewEndTime <= request.NewStartTime)
                throw new BadHttpRequestException("Giờ kết thúc phải sau giờ bắt đầu");

            if (request.NewDate < DateOnly.FromDateTime(DateTime.UtcNow))
                throw new BadHttpRequestException("Không thể đặt lịch trong quá khứ");

            // Kiểm tra double-booking ngày mới (loại trừ chính nó)
            var isDoubleBooked = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == request.NewDate &&
                a.StartTime == request.NewStartTime &&
                a.Id != id &&
                a.Status != "Cancelled" &&
                a.Status != "NoShow");

            if (isDoubleBooked)
                throw new BadHttpRequestException(
                    "Khung giờ mới đã được đặt. Vui lòng chọn giờ khác.");

            // Đánh dấu appointment cũ là Rescheduled
            appointment.Status = "Rescheduled";
            appointment.Notes = string.IsNullOrWhiteSpace(request.Reason)
                                            ? appointment.Notes
                                            : $"[Đổi lịch] {request.Reason}";
            appointment.AppointmentDate = request.NewDate;
            appointment.StartTime = request.NewStartTime;
            appointment.EndTime = request.NewEndTime;
            appointment.UpdatedBy = updatedByUserId;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Appointment rescheduled: Id={Id}, NewDate={Date}, NewStart={Start}",
                id, request.NewDate, request.NewStartTime);

            return await GetByIdAsync(id);
        }

        // HELPER
        private static void EnsureAppointmentIsModifiable(Appointment appointment)
        {
            var modifiableStatuses = new[] { "Scheduled", "Confirmed" };
            if (!modifiableStatuses.Contains(appointment.Status))
                throw new BadHttpRequestException(
                    $"Không thể chỉnh sửa lịch hẹn đang ở trạng thái '{appointment.Status}'");
        }
        private async Task<AppointmentResponse> ChangeStatusAsync(
            Appointment appointment, string newStatus, long updatedByUserId)
        {
            appointment.Status = newStatus;
            appointment.UpdatedBy = updatedByUserId;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Appointment {Id} status → {Status}", appointment.Id, newStatus);

            return await GetByIdAsync(appointment.Id);
        }
        private async Task<Appointment> FindAppointmentOrThrowAsync(long appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                throw new KeyNotFoundException($"Không tìm thấy lịch hẹn với ID {appointmentId}");

            return appointment;
        }
        private static AppointmentResponse MapToResponse(Appointment a) => new()
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Reason = a.Reason,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            
            Patient = new PatientBriefResponse
            {
                Id = a.Patient.Id,
                UserId = a.Patient.UserId,
                UHID = a.Patient.UHID,
                FullName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                PhoneNumber = a.Patient.PhoneNumber
            },
            Doctor = new DoctorBriefResponse
            {
                Id = a.Doctor.Id,
                FullName = a.Doctor.User.FullName,
                Specialization = a.Doctor.Specialization
            },
            Department = new DepartmentBriefResponse
            {
                Id = a.Doctor.Department.Id,
                Name = a.Doctor.Department.Name
            }

        };

        private static AppointmentSummaryResponse MapToSummaryResponse(Appointment a) => new()
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Reason = a.Reason,
            Patient = new PatientBriefResponse
            {
                Id = a.Patient.Id,
                UHID = a.Patient.UHID,

                FullName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                PhoneNumber = a.Patient.PhoneNumber
            },
            Doctor = new DoctorBriefResponse
            {
                Id = a.Doctor.Id,
                FullName = a.Doctor.User.FullName,
                Specialization = a.Doctor.Specialization
            }
        };
        // 


    }
}
