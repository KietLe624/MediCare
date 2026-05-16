using MediCare.API.DTOs;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IPatientService
    {
        Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(PatientQueryParams query);
        Task<PatientResponse> GetByIdAsync(long id);
        Task<PatientResponse> CreateAsync(CreatePatientRequest request, long createdByUserId);
        Task<PatientResponse> UpdateAsync(long id, UpdatePatientRequest request, long updatedByUserId);
        Task<PagedResponse<PatientAppointmentResponse>> GetAppointmentsAsync(long patientId, PatientHistoryQueryParams query);
        Task<PagedResponse<PatientVisitResponse>> GetVisitsAsync(long patientId, PatientHistoryQueryParams query);
        Task<PagedResponse<PatientPrescriptionResponse>> GetPrescriptionsAsync(long patientId, PatientHistoryQueryParams query);
        Task<PagedResponse<PatientInvoiceResponse>> GetInvoicesAsync(long patientId, PatientHistoryQueryParams query);
    }
}
