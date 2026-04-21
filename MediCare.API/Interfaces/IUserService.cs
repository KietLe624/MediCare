using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IUserService
    {
        Task<PagedResponse<UserSummaryResponse>> GetAllAsync(UserQueryParams query);
        Task<UserResponse> GetByIdAsync(long id);
        Task<UpdateProfileRequest> UpdateAsync(long id, UpdateProfileRequest request);
        Task DeleteAsync(long id);
        Task<UserResponse> UpdateRoleAsync(long id, UpdateUserRoleRequest request);
        Task<UserResponse> UpdateActivateAsync(long id, UpdateUserStatusRequest request);
    }
}
