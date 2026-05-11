using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Doctor, Receptionist, Nurse")]
    public class DoctorController : Controller
    {
        public readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(IDoctorService doctorService, ILogger<DoctorController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DTOs.DoctorDTO.DoctorQueryParams query)
        {
            var result = await _doctorService.GetDoctorsAsync(query);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _doctorService.GetDoctorByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DTOs.DoctorDTO.CreateDoctorRequest request)
        {
            var result = await _doctorService.CreateDoctorAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(long id, DTOs.DoctorDTO.UpdateDoctorRequest request)
        {
            var result = await _doctorService.UpdateDoctorAsync(id, request);
            return Ok(result);
        }

        [HttpGet("{id}/schedules")] // id là doctorId
        public async Task<IActionResult> GetSchedules(long id)
        {
            var result = await _doctorService.GetDoctorSchedulesAsync(id);
            return Ok(result);

        }

        [HttpPost("{id}/schedules")]
        public async Task<IActionResult> CreateSchedule(long id, DTOs.DoctorDTO.CreateScheduleRequest request)
        {
            var result = await _doctorService.CreateDoctorScheduleAsync(id, request);
            return Ok(result);
        }

        [HttpPatch("{doctorId}/schedules/{scheduleId}")]
        public async Task<IActionResult> UpdateSchedule(long doctorId, long scheduleId, DTOs.DoctorDTO.UpdateScheduleRequest request)
        {
            var result = await _doctorService.UpdateDoctorScheduleAsync(doctorId, scheduleId, request);
            return Ok(result);
        }
        [HttpDelete("{doctorId}/schedules/{scheduleId}")]
        public async Task<IActionResult> DeleteSchedule(long doctorId, long scheduleId)
        {
            await _doctorService.DeleteDoctorScheduleAsync(doctorId, scheduleId);
            return NoContent();
        }
        [HttpGet("{doctorId}/appointments")]
        public async Task<IActionResult> GetAppointments(long doctorId, [FromQuery] DTOs.DoctorDTO.DoctorAppointmentQueryParams query)
        {
            if (User.IsInRole("Doctor"))
            {
                var currentUserId = GetCurrentUserId();
                var isOwner = await _doctorService.IsDoctorOwnerAsync(doctorId, currentUserId);
                if (!isOwner) return Forbid();
            }

            var result = await _doctorService.GetAppointmentsAsync(doctorId, query);
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
