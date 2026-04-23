using MediCare.API.DTOs;
using static MediCare.API.DTOs.DoctorDTO;

namespace MediCare.API.Interfaces
{
    public interface IDoctorService
    {
        Task<PagedResponse<DoctorSummaryResponse>> GetDoctorsAsync(DoctorQueryParams query);
        Task<DoctorResponse> GetDoctorByIdAsync(long Id);
        Task<DoctorResponse> CreateDoctorAsync(CreateDoctorRequest request);
        Task<DoctorResponse> UpdateDoctorAsync(long Id, UpdateDoctorRequest request);
        Task<bool> DeleteDoctorAsync(long Id);
        Task<List<DoctorScheduleResponse>> GetDoctorSchedulesAsync(long Id);
    }
}
