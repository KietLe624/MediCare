using MediCare.API.DTOs;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Interfaces
{
    public interface IAuditLogService
    {
        Task<PagedResponse<AuditLogResponse>> GetAllAsync(AuditLogQueryParams query);
        Task<AuditLogResponse> GetByIdAsync(long id);
    }
}
