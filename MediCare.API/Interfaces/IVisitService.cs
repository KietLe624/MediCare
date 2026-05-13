using static MediCare.API.DTOs.DoctorDTO;
using static MediCare.API.DTOs.VisitDTO;

namespace MediCare.API.Interfaces
{
    public interface IVisitService
    {
        // VISITS 
        Task<PagedResponse<VisitSummaryResponse>> GetAllAsync(VisitQueryParams query);
        Task<VisitResponse> GetByIdAsync(long visitId);
        Task<VisitResponse> CreateAsync(CreateVisitRequest request, long createdByUserId);
        Task<VisitResponse> UpdateAsync(long visitId, UpdateVisitRequest request, long updatedByUserId);
        Task<VisitResponse> CompleteAsync(long visitId, long updatedByUserId);
        Task<VisitResponse> CancelAsync(long visitId, long updatedByUserId);

        // PRESCRIPTIONS 
        Task<PrescriptionResponse> AddPrescriptionAsync(long visitId, AddPrescriptionRequest request, long createdByUserId);
        Task<PrescriptionResponse> UpdatePrescriptionAsync(long visitId, long prescriptionId, UpdatePrescriptionRequest request, long updatedByUserId);
        Task DeletePrescriptionAsync(long visitId, long prescriptionId);
    }
}
