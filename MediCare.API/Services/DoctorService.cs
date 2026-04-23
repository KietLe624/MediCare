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
        private readonly ILogger<UserService> _logger;
        private readonly AppDbContext _context;
        public DoctorService(UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<UserService> logger, AppDbContext context)
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
                doctorQuery = doctorQuery.Include(d => d.User)
                    .AsQueryable();
            }

            var totalCount = await doctorQuery.CountAsync();

            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);
            var doctors = await doctorQuery
                .Include(d => d.User) 
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
        public Task<DoctorResponse> CreateDoctorAsync(CreateDoctorRequest request)
        {
            // kiểm tra userId có tồn tại không
            var user = _context.Users.FirstOrDefault(u => u.Id == request.UserId);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy user với ID {request.UserId}");

            // kiểm tra nếu user đã có hồ sơ bác sĩ rồi thì không tạo nữa
            if (_context.Doctors.Any(d => d.UserId == request.UserId))
                throw new InvalidOperationException($"User với ID {request.UserId} đã có hồ sơ bác sĩ");

            var doctor = _mapper.Map<Doctor>(request);
            doctor.UserId = user.Id;
            doctor.CreatedAt = DateTime.UtcNow;
            _context.Doctors.Add(doctor); // thêm vào DbContext
            _context.SaveChanges(); // lưu vào database

            _logger.LogInformation(
                "Tạo thành công hồ sơ bác sĩ: Id={Id}, By={UserId}",
                doctor.Id, doctor.UserId);

            return Task.FromResult(_mapper.Map<DoctorResponse>(doctor));
        }
        public async Task<DoctorResponse> UpdateDoctorAsync(long Id, UpdateDoctorRequest request)
        {
            // tìm doctor theo Id, nếu không tìm thấy thì throw
            var doctor = await FindDoctorOrThrowAsync(Id);
            if(null == doctor)
                throw new KeyNotFoundException($"Không tìm thấy hồ sơ bác sĩ với ID {Id}");

            _mapper.Map(request, doctor);
            _context.SaveChanges();
            return _mapper.Map<DoctorResponse>(doctor);
        }

        public Task<bool> DeleteDoctorAsync(long doctorId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DoctorScheduleResponse>> GetDoctorSchedulesAsync(long Id)
        {
            var doctor = await FindDoctorOrThrowAsync(Id);

            var schedules = doctor.Schedules; // lấy lịch trình của bác sĩ

            // Mapping từ List sang List
            return _mapper.Map<List<DoctorScheduleResponse>>(schedules);
        }
        // HELPER
        private async Task<Doctor> FindDoctorOrThrowAsync(long Id)
        {
            //var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            //if (user == null)
            //    throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {userId}");
            //return user;

            var doctor = await _context.Doctors
                .Include(d => d.User) // lấy thông tin user
                .Include(d => d.Schedules) // lịch trình 
                .FirstOrDefaultAsync(d => d.UserId == Id); // Hoặc d.UserId == id (tùy bro thiết kế)

            if (doctor == null)
                throw new KeyNotFoundException("Không tìm thấy hồ sơ bác sĩ!");
            return doctor;

        }
    }
}
