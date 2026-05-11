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
        Task<DoctorScheduleResponse> CreateDoctorScheduleAsync(long Id, CreateScheduleRequest request);
        Task<DoctorScheduleResponse> UpdateDoctorScheduleAsync(long doctorId, long scheduleId, UpdateScheduleRequest request);
        Task DeleteDoctorScheduleAsync(long doctorId, long scheduleId);
        Task<PagedResponse<DoctorAppointmentResponse>> GetAppointmentsAsync(long doctorId, DoctorAppointmentQueryParams query);
        Task<bool> IsDoctorOwnerAsync(long doctorId, long currentUserId);
    }
}
