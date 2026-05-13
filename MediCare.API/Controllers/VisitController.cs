using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static MediCare.API.DTOs.VisitDTO;

namespace MediCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitController : ControllerBase
    {
        private readonly IVisitService _visitService;
        private readonly ILogger<VisitController> _logger;

        public VisitController(IVisitService visitService, ILogger<VisitController> logger)
        {
            _visitService = visitService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Nurse")]
        public async Task<IActionResult> GetAll([FromQuery] VisitQueryParams query)
        {
            var result = await _visitService.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{visitId:long}")]
        [Authorize(Roles = "Admin,Doctor,Nurse")]
        public async Task<IActionResult> GetById(long visitId)
        {
            var visit = await _visitService.GetByIdAsync(visitId);

            // Patient chỉ xem được bệnh án của chính mình
            if (User.IsInRole("Patient"))
            {
                var currentUserId = GetCurrentUserId();
                if (visit.Patient.UserId != currentUserId)
                    return Forbid();
            }

            return Ok(visit);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create([FromBody] CreateVisitRequest request)
        {
            var createdByUserId = GetCurrentUserId();
            var result = await _visitService.CreateAsync(request, createdByUserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{visitId:long}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Update(long visitId, [FromBody] UpdateVisitRequest request)
        {
            var updatedByUserId = GetCurrentUserId();
            var result = await _visitService.UpdateAsync(visitId, request, updatedByUserId);
            return Ok(result);
        }

        [HttpPatch("{visitId:long}/complete")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Complete(long visitId)
        {
            var result = await _visitService.CancelAsync(visitId, GetCurrentUserId());
            return Ok(result);
        }

        [HttpPatch("{visitId:long}/cancel")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Cancel(long visitId)
        {
            var result = await _visitService.CancelAsync(visitId, GetCurrentUserId());
            return Ok(result);
        }

        // PRESCRIPTION 
        [HttpPost("{visitId:long}/prescriptions")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> AddPrescriptionAsync(
            long visitId, [FromBody] AddPrescriptionRequest request)
        {
            var createdByUserId = GetCurrentUserId();
            var result = await _visitService.AddPrescriptionAsync(visitId, request, createdByUserId);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPatch("{visitId:long}/prescriptions/{prescriptionId:long}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdatePrescription(
            long visitId, long prescriptionId, [FromBody] UpdatePrescriptionRequest request)
        {
            var updatedByUserId = GetCurrentUserId();
            var result = await _visitService.UpdatePrescriptionAsync(
                visitId, prescriptionId, request, updatedByUserId);
            return Ok(result);
        }

        [HttpDelete("{visitId:long}/prescriptions/{prescriptionId:long}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeletePrescription(long visitId, long prescriptionId)
        {
            await _visitService.DeletePrescriptionAsync(visitId, prescriptionId);
            return NoContent();
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
