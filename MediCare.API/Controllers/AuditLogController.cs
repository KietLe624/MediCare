using MediCare.API.DTOs;
using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static MediCare.API.DTOs.UserDTO;

namespace MediCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>Danh sách audit log (filter, phân trang)</summary>
        /// <remarks>
        /// Roles: Admin
        ///
        /// Query params:
        /// - userId    : lọc theo user thực hiện
        /// - action    : Create | Update | Delete | Login | Logout
        /// - entityName: Patient | Appointment | Visit | Prescription | Invoice...
        /// - fromDate  : từ ngày (yyyy-MM-dd)
        /// - toDate    : đến ngày (yyyy-MM-dd)
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AuditLogQueryParams query)
        {
            var result = await _auditLogService.GetAllAsync(query);
            return Ok(result);
        }

        /// <summary>Chi tiết 1 audit log (có OldValues / NewValues đầy đủ)</summary>
        [HttpGet("{auditLogId:long}")]
        public async Task<IActionResult> GetById(long auditLogId)
        {
            var result = await _auditLogService.GetByIdAsync(auditLogId);
            return Ok(result);
        }
    }
}
