using static MediCare.API.DTOs.AppointmentDTO;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IAppointmentService
    {
        Task<PagedResponse<AppointmentSummaryResponse>> GetAllAsync(AppointmentQueryParams query);
        Task<AppointmentResponse> GetByIdAsync(long id);
        Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, long createdByUserId);
        Task<AppointmentResponse> UpdateAsync(long id, UpdateAppointmentRequest request, long updatedByUserId);

        // Các thao tác đổi trạng thái — dùng PATCH
        Task<AppointmentResponse> ConfirmAsync(long id, long updatedByUserId);
        Task<AppointmentResponse> CompleteAsync(long id, long updatedByUserId);
        Task<AppointmentResponse> CancelAsync(long id, CancelAppointmentRequest request, long updatedByUserId);
        Task<AppointmentResponse> NoShowAsync(long id, long updatedByUserId);
        Task<AppointmentResponse> RescheduleAsync(long id, RescheduleAppointmentRequest request, long updatedByUserId);
    }
}
