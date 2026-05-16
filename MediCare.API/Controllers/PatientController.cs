using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientController> _logger;
        public PatientController(IPatientService patientService, ILogger<PatientController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")] // Admin, Doctor, Nurse đều có thể xem danh sách bệnh nhân
        public async Task<IActionResult> GetAll([FromQuery] DTOs.PatientQueryParams query)
        {
            var result = await _patientService.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _patientService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nurse,Receptionist")]
        public async Task<IActionResult> Create(DTOs.CreatePatientRequest request)
        {
            var createdByUserId = GetCurrentUserId();
            var result = await _patientService.CreateAsync(request, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Nurse,Receptionist")]
        public async Task<IActionResult> Update(long id, DTOs.UpdatePatientRequest request)
        {
            var updatedByUserId = GetCurrentUserId();
            var result = await _patientService.UpdateAsync(id, request, updatedByUserId);
            return Ok(result);
        }

        [HttpGet("appointments")]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")] // Admin, Doctor, Nurse đều có thể xem lịch sử hẹn khám
        public async Task<IActionResult> GetAppointments(long patientId, [FromQuery] DTOs.PatientHistoryQueryParams query)
        {
            var result = await _patientService.GetAppointmentsAsync(patientId, query);
            return Ok(result);
        }

        [HttpGet("visits")]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")] // Admin, Doctor, Nurse đều có thể xem lịch sử khám bệnh
        public async Task<IActionResult> GetVisits(long patientId, [FromQuery] DTOs
            .PatientHistoryQueryParams query)
        {
            var result = await _patientService.GetVisitsAsync(patientId, query);
            return Ok(result);
        }

        [HttpGet("prescriptions")]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")] // Admin, Doctor, Nurse đều có thể xem đơn thuốc
        public async Task<IActionResult> GetPrescriptions(long patientId, [FromQuery] DTOs.PatientHistoryQueryParams query)
        {
            var result = await _patientService.GetPrescriptionsAsync(patientId, query);
            return Ok(result);
        }

        [HttpGet("invoices")]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")] // Admin, Doctor, Nurse đều có thể xem hóa đơn
        public async Task<IActionResult> GetInvoices(long patientId, [FromQuery] DTOs.PatientHistoryQueryParams query)
        {
            var result = await _patientService.GetInvoicesAsync(patientId, query);
            return Ok(result);
        }

        // HEPLER 
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
