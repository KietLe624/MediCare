using AutoMapper;
using MediCare.API.Data;
using MediCare.API.DTOs;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.DoctorDTO;

namespace MediCare.API.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorService> _logger;
        private readonly AppDbContext _context;
        public DoctorService(UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<DoctorService> logger, AppDbContext context)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public async Task<PagedResponse<DoctorSummaryResponse>> GetDoctorsAsync(DoctorQueryParams query)
        {
            // lấy danh sách bác sĩ từ database, áp dụng filter, search, pagination
            var doctorQuery = _context.Doctors.AsNoTracking();

            // áp dụng filter, search, pagination ở đây
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchLower = query.Search.ToLower(); // convert sang chữ thường
                doctorQuery = doctorQuery
                    .Include(d => d.User)
                    .Where(d => d.User.FullName.ToLower().Contains(searchLower) ||
                    (d.Specialization != null &&
                     d.Specialization.ToLower().Contains(searchLower)));
            }

            var totalCount = await doctorQuery.CountAsync();

            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);
            var doctors = await doctorQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = _mapper.Map<List<DoctorSummaryResponse>>(doctors); // map sang DTO trả về

            return new PagedResponse<DoctorSummaryResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<DoctorResponse> GetDoctorByIdAsync(long Id)
        {
            var doctor = await FindDoctorOrThrowAsync(Id);

            return _mapper.Map<DoctorResponse>(doctor);
        }
        public async Task<DoctorResponse> CreateDoctorAsync(CreateDoctorRequest request)
        {
            // kiểm tra userId có tồn tại không
            var user = await FindDoctorOrThrowAsync(request.UserId);

            var deptExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId);
            if (!deptExists)
                throw new BadHttpRequestException($"DepartmentId = {request.DepartmentId} không tồn tại");

            // kiểm tra nếu user đã có hồ sơ bác sĩ rồi thì không tạo nữa
            if (await _context.Doctors.AnyAsync(d => d.UserId == request.UserId))
                throw new InvalidOperationException($"User với ID {request.UserId} đã có hồ sơ bác sĩ");

            var doctor = _mapper.Map<Doctor>(request);
            doctor.UserId = user.Id;
            doctor.CreatedAt = DateTime.UtcNow;
            _context.Doctors.Add(doctor); // thêm vào DbContext
            await _context.SaveChangesAsync(); // lưu vào database

            _logger.LogInformation(
                "Tạo thành công hồ sơ bác sĩ: Id={Id}, By={UserId}",
                doctor.Id, doctor.UserId);

            return _mapper.Map<DoctorResponse>(doctor);
        }
        public async Task<DoctorResponse> UpdateDoctorAsync(long Id, UpdateDoctorRequest request)
        {
            // tìm doctor theo Id, nếu không tìm thấy thì throw
            var doctor = await FindDoctorOrThrowAsync(Id);
            _mapper.Map(request, doctor);
            await _context.SaveChangesAsync();
            return _mapper.Map<DoctorResponse>(doctor);
        }
        public Task<bool> DeleteDoctorAsync(long doctorId)
        {
            throw new NotImplementedException();
        }
        // SCHEDULE
        public async Task<List<DoctorScheduleResponse>> GetDoctorSchedulesAsync(long Id)
        {
            var doctor = await FindDoctorOrThrowAsync(Id);

            var schedules = await _context.DoctorSchedules
                .AsNoTracking()
                .Where(s => s.DoctorId == Id)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();



            // Mapping từ List sang List
            return _mapper.Map<List<DoctorScheduleResponse>>(schedules);
        }
        public async Task<DoctorScheduleResponse> CreateDoctorScheduleAsync(long Id, CreateScheduleRequest request)
        {
            var doctor = await FindDoctorOrThrowAsync(Id);

            // Validate giờ kết thúc phải sau giờ bắt đầu
            if (request.EndTime <= request.StartTime)
                throw new BadHttpRequestException("Giờ kết thúc phải sau giờ bắt đầu");

            // kiểm tra nếu lịch trình trùng với lịch trình hiện có của bác sĩ thì không tạo được
            var overlapping = await _context.DoctorSchedules.AnyAsync(s =>
                s.DayOfWeek == request.DayOfWeek &&
                s.StartTime < request.EndTime &&
                s.EndTime > request.StartTime &&
                s.IsActive
            );

            if (overlapping)
                throw new BadHttpRequestException("Lịch trình trùng với lịch trình hiện có của bác sĩ");

            var schedule = new DoctorSchedule
            {
                DoctorId = Id, // khóa ngoại liên kết với Doctor
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                //SlotDurationMinutes = request.SlotDurationMinutes,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.DoctorSchedules.Add(schedule); // thêm vào DbContext
            await _context.SaveChangesAsync(); // lưu vào database
            _logger.LogInformation(
               "Schedule created: DoctorId={DoctorId}, Day={Day}, {Start}-{End}",
               Id, request.DayOfWeek, request.StartTime, request.EndTime);

            return _mapper.Map<DoctorScheduleResponse>(schedule);
        }
        public async Task<DoctorScheduleResponse> UpdateDoctorScheduleAsync(long doctorId, long scheduleId, UpdateScheduleRequest request)
        {
            var doctor = await FindDoctorOrThrowAsync(doctorId);
            var schedule = await FindScheduleOrThrowAsync(doctorId, scheduleId);

            // Validate giờ kết thúc phải sau giờ bắt đầu
            if (request.EndTime <= request.StartTime)
                throw new BadHttpRequestException("Giờ kết thúc phải sau giờ bắt đầu");
            // kiểm tra nếu lịch trình trùng với lịch trình hiện có của bác sĩ thì không tạo được
            var overlapping = await _context.DoctorSchedules.AnyAsync(s =>
                s.Id != scheduleId && // loại trừ chính nó
                s.DayOfWeek == request.DayOfWeek &&
                s.StartTime < request.EndTime &&
                s.EndTime > request.StartTime &&
                s.IsActive
            );

            if (overlapping)
                throw new BadHttpRequestException("Lịch trình trùng với lịch trình hiện có của bác sĩ");

            schedule.DayOfWeek = request.DayOfWeek;
            schedule.StartTime = request.StartTime;
            schedule.EndTime = request.EndTime;
            schedule.IsActive = request.IsActive;

            _mapper.Map(request, schedule); // map các trường cập nhật vào entity
            await _context.SaveChangesAsync(); // lưu vào database
            _logger.LogInformation(
               "Schedule updated: DoctorId={DoctorId}, ScheduleId={ScheduleId}, Day={Day}, {Start}-{End}",
               doctorId, scheduleId, request.DayOfWeek, request.StartTime, request.EndTime);
            return _mapper.Map<DoctorScheduleResponse>(schedule);
        }

        // xóa lịch bác sĩ
        public async Task DeleteDoctorScheduleAsync(long doctorId, long scheduleId)
        {
            var doctor = await FindDoctorOrThrowAsync(doctorId);
            var schedule = await FindScheduleOrThrowAsync(doctorId, scheduleId);

            _context.DoctorSchedules.Remove(schedule); // xóa khỏi DbContext
            await _context.SaveChangesAsync(); // lưu vào database

            _logger.LogInformation(
               "Schedule deleted: DoctorId={DoctorId}, ScheduleId={ScheduleId}",
               doctorId, scheduleId);
        }
        // APPOINTMENT
        public async Task<PagedResponse<DoctorAppointmentResponse>> GetAppointmentsAsync(long doctorId, DoctorAppointmentQueryParams query)
        {
            var doctorSchedule = await FindDoctorOrThrowAsync(doctorId);

            var q = _context.Appointments
               .AsNoTracking()
               .Where(a => a.DoctorId == doctorId)
               .Include(a => a.Patient);

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);


            var appointments = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var items = appointments.Select(a => new DoctorAppointmentResponse
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Reason = a.Reason,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                Patient = new PatientBriefResponse
                {
                    Id = a.Patient.Id,
                    UHID = a.Patient.UHID,
                    FullName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    PhoneNumber = a.Patient.PhoneNumber
                }
            }).ToList();

            
            return new PagedResponse<DoctorAppointmentResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        // HELPER
        private async Task<Doctor> FindDoctorOrThrowAsync(long Id)
        {

            var doctor = await _context.Doctors
                .Include(d => d.User) // lấy thông tin user
                .Include(d => d.Schedules) // lịch trình 
                .FirstOrDefaultAsync(d => d.Id == Id);

            if (doctor == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ bác sĩ!");
            return doctor;

        }
        private async Task<DoctorSchedule> FindScheduleOrThrowAsync(long doctorId, long scheduleId)
        {
            var schedule = await _context.DoctorSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.DoctorId == doctorId);
            if (schedule == null)
                throw new KeyNotFoundException("Không tìm thấy lịch trình bác sĩ!");
            return schedule;
        }
        public async Task<bool> IsDoctorOwnerAsync(long doctorId, long userId)
        {
            return await _context.Doctors
                .AnyAsync(d => d.Id == doctorId && d.UserId == userId);
        }
    }
}
