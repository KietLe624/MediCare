using static MediCare.API.DTOs.MedicationDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IMedicationService
    {
        Task<PagedResponse<MedicationResponse>> GetAllAsync(MedicationQueryParams query);
        Task<MedicationResponse> GetByIdAsync(long id);
        Task<MedicationResponse> CreateAsync(CreateMedicationRequest request);
        Task<MedicationResponse> UpdateAsync(long id, UpdateMedicationRequest request);
        Task<MedicationResponse> ActiveAsync(long id);
        Task<List<MedicationCategoryResponse>> GetCategoriesAsync();   // danh sách danh mục
    }
}
