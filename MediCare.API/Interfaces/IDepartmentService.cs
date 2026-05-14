using static MediCare.API.DTOs.DepartmentDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IDepartmentService
    {
        Task<PagedResponse<DepartmentResponse>> GetAllAsync(DepartmentQueryParams query);
        Task<DepartmentDetailResponse> GetByIdAsync(long id);
        Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request);
        Task<DepartmentResponse> UpdateAsync(long id, UpdateDepartmentRequest request);
        Task DeleteAsync(long id);
    }
}
