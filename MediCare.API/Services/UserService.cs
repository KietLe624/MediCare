using AutoMapper;
using MediCare.API.Data;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly AppDbContext _context;
        private static readonly string[] AllowedRoles =
            { "Admin", "Doctor", "Nurse", "Receptionist", "Patient" };

        // Constructor
        public UserService(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ILogger<UserService> logger,
            AppDbContext context ) 
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        // GET ALL USERS
        public async Task<PagedResponse<UserSummaryResponse>> GetAllAsync(UserQueryParams query)
        {
            var usersQuery = _userManager.Users.AsNoTracking();

            // Search đơn giản
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();

                usersQuery = usersQuery.Where(u =>
                    u.FullName.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword) ||
                    u.UserName.ToLower().Contains(keyword));
            }

            // Total count
            var totalCount = await usersQuery.CountAsync();

            // Pagination (bảo vệ input)
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var users = await usersQuery
                .OrderBy(u => u.Id) // đơn giản thôi
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map DTO + Roles
            var items = new List<UserSummaryResponse>();

            foreach (var user in users)
            {
                var role = await _userManager.GetRolesAsync(user);

                items.Add(new UserSummaryResponse
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    Role = role.FirstOrDefault() // Chỉ lấy role đầu tiên
                });
            }

            return new PagedResponse<UserSummaryResponse>
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // GET USER BY ID
        public async Task<UserResponse?> GetByIdAsync(long id)
        {
            // lấy user
            var user = await FindUserByIdAsync(id);
            // map DTO
            var dto = _mapper.Map<UserResponse>(user);
            return dto;
        }

        // UPDATE USER INFO
        public async Task<UpdateProfileRequest?> UpdateAsync(long id, UpdateProfileRequest request)
        {
            var user = await FindUserByIdAsync(id);

            // thông tin update
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;

            // kiểm tra email, sđt
            if (await _userManager.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                throw new InvalidOperationException("Email đã được sử dụng.");
            if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != id))
                throw new InvalidOperationException("Số điện thoại đã được sử dụng.");

            _mapper.Map(request, user);
            await _userManager.UpdateAsync(user);
            return _mapper.Map<UpdateProfileRequest>(user);
        }

        // DELETE USER(soft delete)
        public async Task DeleteAsync(long id)
        {
            var user = await FindUserByIdAsync(id);
            user.IsActive = false; // soft delete

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadHttpRequestException(errors);
            }
            _logger.LogInformation("Người dùng với {ID} đã được xoá.", id);
        }

        // UPDATE USER ROLES
        public async Task<UserResponse> UpdateRoleAsync(long id, UpdateUserRoleRequest request)
        {
            // Tìm user
            var user = await FindUserByIdAsync(id);

            // Kiểm tra role duy nhất có hợp lệ không
            if (!AllowedRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
            {
                throw new BadHttpRequestException(
                    $"Role không hợp lệ: {request.Role}. " +
                    $"Các role cho phép: {string.Join(", ", AllowedRoles)}");
            }

            // Lấy các roles hiện tại
            var currentRoles = await _userManager.GetRolesAsync(user);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa tất cả role cũ (nếu có) để đảm bảo user chỉ có 1 role duy nhất
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                        throw new BadHttpRequestException(errors);
                    }
                }

                // Thêm role mới (Sử dụng AddToRoleAsync thay vì AddToRolesAsync)
                var addResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    throw new BadHttpRequestException(errors);
                }

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating user role for user {UserId}", user.Id);
                await transaction.RollbackAsync();
                throw;
            }

            _logger.LogInformation(
                "Role updated for user {UserId}: {Role}",
                id, request.Role);

            var dto = _mapper.Map<UserResponse>(user);

            // Lưu ý: Cập nhật property trong UserResponse DTO thành chuỗi đơn 'Role'
            dto.Role = request.Role;

            return dto;
        }

        // ACTIVE ACCOUNT
        public async Task<UserResponse> UpdateActivateAsync(long id, UpdateUserStatusRequest request)
        {
            var user = await FindUserByIdAsync(id);

            user.IsActive = request.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadHttpRequestException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            _logger.LogInformation(
                "User {UserId} {Action}",
                id, request.IsActive ? "activated" : "deactivated");

            var dto = _mapper.Map<UserResponse>(user);
            dto.IsActive = request.IsActive;
            return dto;

        }

        // HELPER 
        public async Task<ApplicationUser?> FindUserByIdAsync(long id)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {id}");
            return user;
        }
    }
}
