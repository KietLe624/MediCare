using MediCare.API.Data;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.DepartmentDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Services
{
    public class DepartmentService : IDepartmentService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(AppDbContext context, ILogger<DepartmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET ALL 
        public async Task<PagedResponse<DepartmentResponse>> GetAllAsync(DepartmentQueryParams query)
        {
            var q = _context.Departments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                q = q.Where(d =>
                    d.Name.ToLower().Contains(keyword) ||
                    (d.Description != null &&
                     d.Description.ToLower().Contains(keyword)));
            }

            q = query.SortBy.ToLower() switch
            {
                "createdat" => query.SortOrder == "asc"
                                ? q.OrderBy(d => d.CreatedAt)
                                : q.OrderByDescending(d => d.CreatedAt),
                _ => query.SortOrder == "asc"
                                ? q.OrderBy(d => d.Name)
                                : q.OrderByDescending(d => d.Name)
            };

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var departments = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Đếm số bác sĩ của từng khoa
            var deptIds = departments.Select(d => d.Id).ToList();
            var doctorCounts = await _context.Doctors
                .Where(d => deptIds.Contains(d.DepartmentId))
                .GroupBy(d => d.DepartmentId)
                .Select(g => new { DeptId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DeptId, x => x.Count); // deptId -> doctor count

            var items = departments.Select(d => new DepartmentResponse
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                CreatedAt = d.CreatedAt,
                DoctorCount = doctorCounts.GetValueOrDefault(d.Id, 0) // count 
            }).ToList();

            return new PagedResponse<DepartmentResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // GET BY ID

        public async Task<DepartmentDetailResponse> GetByIdAsync(long id)
        {
            // thông tin doctor + user 
            var department = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Doctors)
                    .ThenInclude(doc => doc.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                throw new KeyNotFoundException($"Không tìm thấy khoa với ID {id}");

            return new DepartmentDetailResponse
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                CreatedAt = department.CreatedAt,
                DoctorCount = department.Doctors.Count,
                Doctors = department.Doctors
                    .OrderBy(d => d.User.FullName)
                    .Select(d => new DoctorBriefResponse
                    {
                        Id = d.Id,
                        FullName = d.User.FullName,
                        Specialization = d.Specialization,
                        ConsultationFee = d.ConsultationFee,
                        IsAvailable = d.IsAvailable
                    }).ToList()
            };
        }
        // CREATE
        public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request)
        {
            // Tên khoa phải duy nhất
            var nameExists = await _context.Departments
                .AnyAsync(d => d.Name.ToLower() == request.Name.ToLower());
            if (nameExists)
                throw new BadHttpRequestException($"Khoa '{request.Name}' đã tồn tại");

            var department = new Department
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Department created: Id={Id}, Name={Name}", department.Id, department.Name);

            return new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                CreatedAt = department.CreatedAt,
                DoctorCount = 0 // mới tạo, chưa có bác sĩ nào
            };
        }

        // UPDATE
        public async Task<DepartmentResponse> UpdateAsync(long id, UpdateDepartmentRequest request)
        {
            var department = await FindDepartmentOrThrowAsync(id);

            // Kiểm tra tên mới không trùng với khoa khác
            var nameExists = await _context.Departments
                .AnyAsync(d => d.Name.ToLower() == request.Name.ToLower() && d.Id != id);
            if (nameExists)
                throw new BadHttpRequestException(
                    $"Tên khoa '{request.Name}' đã được sử dụng");

            department.Name = request.Name.Trim();
            department.Description = request.Description?.Trim();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Department updated: Id={Id}, Name={Name}", id, department.Name);

            // Đếm lại số bác sĩ sau khi update
            var doctorCount = await _context.Doctors
                .CountAsync(d => d.DepartmentId == id);

            return new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                CreatedAt = department.CreatedAt,
                DoctorCount = doctorCount
            };
        }

        // DELETE
        public async Task DeleteAsync(long id)
        {
            var department = await FindDepartmentOrThrowAsync(id);

            // Không xóa khoa còn bác sĩ — tránh orphan Doctor records
            var hasDoctors = await _context.Doctors.AnyAsync(d => d.DepartmentId == id);
            if (hasDoctors)
                throw new BadHttpRequestException(
                    "Không thể xóa khoa còn bác sĩ. Vui lòng chuyển bác sĩ sang khoa khác trước.");

            // Hard delete — Department không có dữ liệu lịch sử cần bảo toàn
            // (Appointment lưu DoctorId, không lưu DepartmentId trực tiếp)
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            _logger.LogWarning("Department deleted: Id={Id}, Name={Name}", id, department.Name);
        }


        // HELPERS
        private async Task<Department> FindDepartmentOrThrowAsync(long id)
        {
            var department = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                throw new KeyNotFoundException($"Không tìm thấy khoa với ID {id}");

            return department;
        }
    }
}
