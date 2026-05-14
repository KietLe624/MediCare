using MediCare.API.Data;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.MedicationDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MedicationService> _logger;

        public MedicationService(AppDbContext context, ILogger<MedicationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET ALL 
        public async Task<PagedResponse<MedicationResponse>> GetAllAsync(MedicationQueryParams query)
        {
            var q = _context.Medications.AsNoTracking(); // lấy dữ liệu thuốc

            // FILTER
            // Theo tên
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                q = q.Where(m =>
                    m.Name.ToLower().Contains(keyword) ||
                    (m.GenericName != null &&
                     m.GenericName.ToLower().Contains(keyword)));
            }
            // Theo danh mục
            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                var category = query.Category.Trim().ToLower();
                q = q.Where(m => m.Category != null && m.Category.ToLower() == category);
            }
            // Theo trạng thái
            if (query.IsActive.HasValue)
            {
                q = q.Where(m => m.IsActive == query.IsActive.Value);
            }
            // SORT: Theo 
            q = query.SortBy.ToLower() switch
            {
                "genericname" => query.SortOrder == "asc"
                                    ? q.OrderBy(m => m.GenericName)
                                    : q.OrderByDescending(m => m.GenericName),
                "category" => query.SortOrder == "asc"
                                    ? q.OrderBy(m => m.Category)
                                    : q.OrderByDescending(m => m.Category),
                "createdat" => query.SortOrder == "asc"
                                    ? q.OrderBy(m => m.CreatedAt)
                                    : q.OrderByDescending(m => m.CreatedAt),
                _ => query.SortOrder == "asc"
                                    ? q.OrderBy(m => m.Name)
                                    : q.OrderByDescending(m => m.Name)
            };

            var totalCount = await q.CountAsync();
            var pageSize = Math.Min(query.PageSize, 100);
            var page = Math.Max(query.Page, 1);

            var medications = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<MedicationResponse>
            {
                Data = medications.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

        }
        // GET BY ID
        public async Task<MedicationResponse> GetByIdAsync(long id)
        {
            var medication = await FindMedicationOrThrowAsync(id);
            return MapToResponse(medication);
        }
        // CREATE
        public async Task<MedicationResponse> CreateAsync(CreateMedicationRequest request)
        {
            // Kiểm tra tên thuốc không trùng
            var nameExists = await _context.Medications
                .AnyAsync(m => m.Name.ToLower() == request.Name.ToLower());

            if (nameExists)
                throw new BadHttpRequestException(
                    $"Thuốc '{request.Name}' đã tồn tại trong hệ thống");

            var medication = new Medication
            {
                Name = request.Name.Trim(),
                GenericName = request.GenericName?.Trim(),
                Category = request.Category?.Trim(),
                Unit = request.Unit?.Trim(),
                Description = request.Description?.Trim(),
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Tạo thành công: Id={Id}, Name={Name}, Category={Category}",
                medication.Id, medication.Name, medication.Category);

            return MapToResponse(medication);
        }

        // UPDATE
        public async Task<MedicationResponse> UpdateAsync(long id, UpdateMedicationRequest request)
        {
            var medication = await FindMedicationOrThrowAsync(id);

            // Kiểm tra tên mới không trùng với thuốc khác
            var nameExists = await _context.Medications
                .AnyAsync(m =>
                    m.Name.ToLower() == request.Name.ToLower() &&
                    m.Id != id);
            if (nameExists)
                throw new BadHttpRequestException(
                    $"Tên thuốc '{request.Name}' đã được sử dụng bởi thuốc khác");

            medication.Name = request.Name.Trim();
            medication.GenericName = request.GenericName?.Trim();
            medication.Category = request.Category?.Trim();
            medication.Unit = request.Unit?.Trim();
            medication.Description = request.Description?.Trim();
            medication.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã chỉnh sửa: Id={Id}, Name={Name}", id, medication.Name);

            return MapToResponse(medication);
        }

        /// <summary>
        ///     Chuyển trạng thái nếu không còn sử dụng nữa (IsActive = false)
        ///     Không xoá khỏi hệ thống tránh mất dữ liệu lịch sử kê đơn
        /// </summary>
        public async Task<MedicationResponse> ActiveAsync(long id)
        {
            var medication = await FindMedicationOrThrowAsync(id);

            // Nếu đang deactivate, kiểm tra có prescription đang dùng không
            if (medication.IsActive)
            {
                var inUsed = await _context.Prescriptions
                    .AnyAsync(p =>
                        p.MedicationId == id &&
                        p.Visit.Status == "InProgress");  // chỉ block nếu đang trong lần khám

                if (inUsed)
                    throw new BadHttpRequestException(
                        "Không thể ngừng thuốc đang được kê trong bệnh án chưa hoàn thành");
            }

            medication.IsActive = !medication.IsActive;
            await _context.SaveChangesAsync();

            var action = medication.IsActive ? "activated" : "deactivated";
            _logger.LogInformation("Medication {Id} {Action}", id, action);

            return MapToResponse(medication);
        }

        // GET CATEGORIES
        public async Task<List<MedicationCategoryResponse>> GetCategoriesAsync()
        {
            // Lấy danh sách danh mục + đếm số thuốc active trong mỗi danh mục
            var categories = await _context.Medications
                .AsNoTracking()
                .Where(m => m.Category != null && m.IsActive)
                .GroupBy(m => m.Category!)
                .Select(g => new MedicationCategoryResponse
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories;
        }
        // HELPER
        private async Task<Medication> FindMedicationOrThrowAsync(long id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
                throw new KeyNotFoundException($"Không tìm thấy thuốc với ID {id}");
            return medication;
        }

        private static MedicationResponse MapToResponse(Medication m) => new()
        {
            Id = m.Id,
            Name = m.Name,
            GenericName = m.GenericName,
            Category = m.Category,
            Unit = m.Unit,
            Description = m.Description,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt
        };
    }
}
