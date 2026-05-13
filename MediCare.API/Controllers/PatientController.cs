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
        public async Task<IActionResult> GetAll([FromQuery] DTOs.PatientDTO.PatientQueryParams query)
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
        public async Task<IActionResult> CreatePatient(DTOs.PatientDTO.CreatePatientRequest request)
        {
            var createdByUserId = GetCurrentUserId();
            var result = await _patientService.CreateAsync(request, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Nurse,Receptionist")]
        public async Task<IActionResult> Update(long id, DTOs.PatientDTO.UpdatePatientRequest request)
        {
            var updatedByUserId = GetCurrentUserId();
            var result = await _patientService.UpdateAsync(id, request, updatedByUserId);
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
