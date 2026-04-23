using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Doctor,Receptionist, Nurse")]
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

        [HttpGet("{id}/schedules")]
        public async Task<IActionResult> GetSchedules(long id)
        {
            var result = await _doctorService.GetDoctorSchedulesAsync(id);
            return Ok(result);

        }
    }
}
