using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static MediCare.API.DTOs.AppointmentDTO;

namespace MediCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class AppointmentController : ControllerBase
    {
        private readonly ILogger<AppointmentController> _logger;
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(ILogger<AppointmentController> logger, IAppointmentService appointmentService)
        {
            _logger = logger;
            _appointmentService = appointmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist,Doctor,Nurse")]
        public async Task<IActionResult> GetAll([FromQuery] AppointmentQueryParams query)
        {
            var result = await _appointmentService.GetAllAsync(query);
            return Ok(result);
        }
        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetById(long appointmentId)
        {
            var appointment = await _appointmentService.GetByIdAsync(appointmentId);

            // Patient chỉ xem được lịch của mình
            if (User.IsInRole("Patient")) 
            {
                var currentUserId = GetCurrentUserId();
                if (appointment.Patient.UserId != currentUserId)
                    return Forbid();
            }

            return Ok(appointment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist,Patient")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var createdByUserId = GetCurrentUserId();
            var result = await _appointmentService.CreateAsync(request, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{appointmentId:long}")]
        [Authorize(Roles = "Admin,Receptionist,Patient")]
        public async Task<IActionResult> Update(long appointmentId, [FromBody] UpdateAppointmentRequest request)
        {
            var updatedByUserId = GetCurrentUserId();
            var result = await _appointmentService.UpdateAsync(appointmentId, request, updatedByUserId);
            return Ok(result);
        }

        [HttpPatch("{id:long}/confirm")]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public async Task<IActionResult> Confirm(long id)
        {
            var result = await _appointmentService.ConfirmAsync(id, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPatch("{id:long}/complete")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Complete(long id)
        {
            var result = await _appointmentService.CompleteAsync(id, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPatch("{id:long}/cancel")]
        public async Task<IActionResult> Cancel(long id, [FromBody] CancelAppointmentRequest request)
        {
            // Patient chỉ hủy được lịch của chính mình
            if (User.IsInRole("Patient"))
            {
                var appointment = await _appointmentService.GetByIdAsync(id);
                if (appointment.Patient.UserId != GetCurrentUserId())
                    return Forbid();
            }

            var result = await _appointmentService.CancelAsync(id, request, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPatch("{id:long}/no-show")]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public async Task<IActionResult> NoShow(long id)
        {
            var result = await _appointmentService.NoShowAsync(id, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPatch("{id:long}/reschedule")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Reschedule(
            long id, [FromBody] RescheduleAppointmentRequest request)
        {
            var result = await _appointmentService.RescheduleAsync(id, request, GetCurrentUserId());
            return Ok(result);
        }
        // HELPER
        private long GetCurrentUserId()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Không xác định được người dùng");

            return userId;
        }
    } 
}
