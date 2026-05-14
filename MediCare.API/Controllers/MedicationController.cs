using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static MediCare.API.DTOs.MedicationDTO;

namespace MediCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationController : ControllerBase
    {
        private readonly IMedicationService _medicationService;
        private readonly ILogger<MedicationController> _logger;

        public MedicationController(IMedicationService medicationService, ILogger<MedicationController> logger)
        {
            _medicationService = medicationService;
            _logger = logger;
        }


        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")]
        public async Task<IActionResult> GetAll([FromQuery] MedicationQueryParams query)
        {
            var result = await _medicationService.GetAllAsync(query);
            return Ok(result);

        }
        [HttpGet("{id:long}")]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _medicationService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("categories")]
        [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _medicationService.GetCategoriesAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMedicationRequest request)
        {
            var result = await _medicationService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateMedicationRequest request)
        {
            var result = await _medicationService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpPatch("{id:long}/active")]
        public async Task<IActionResult> ToggleActive(long id)
        {
            var result = await _medicationService.ActiveAsync(id);
            return Ok(result);
        }
    }
}
