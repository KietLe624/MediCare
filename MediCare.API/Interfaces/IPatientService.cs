using MediCare.API.DTOs;
using static MediCare.API.DTOs.PatientDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IPatientService
    {
        Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(PatientQueryParams query);
        Task<PatientResponse> GetByIdAsync(long id);
        Task<PatientResponse> CreateAsync(CreatePatientRequest request, long createdByUserId);
        Task<PatientResponse> UpdateAsync(long id, UpdatePatientRequest request, long updatedByUserId);
    }
}
