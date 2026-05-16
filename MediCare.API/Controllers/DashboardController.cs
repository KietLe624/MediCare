using MediCare.API.Data;
using MediCare.API.DTOs;
using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MediCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;
        private readonly AppDbContext _context;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger, AppDbContext context)
        {
            _dashboardService = dashboardService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("overview")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _dashboardService.GetOverviewAsync();
            return Ok(result);
        }

        [HttpGet("appointments-today")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetAppointmentsToday()
        {
            var result = await _dashboardService.GetAppointmentsTodayAsync();
            return Ok(result);
        }

        [HttpGet("patients/by-department")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPatientsByDepartment()
        {
            var result = await _dashboardService.GetPatientsByDepartmentAsync();
            return Ok(result);
        }

        [HttpGet("visits/by-date")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetVisitsByDate([FromQuery] int days = 7)
        {
            var result = await _dashboardService.GetVisitsByDateAsync(days);
            return Ok(result);
        }

        // REVENUE
        [HttpGet("revenue/by-date")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueByDate([FromQuery] int days = 7)
        {
            var result = await _dashboardService.GetRevenueByDateAsync(days);
            return Ok(result);
        }
        /// <summary>Doanh thu theo tháng trong năm — dùng cho biểu đồ cột</summary>
        /// <remarks>
        /// Roles: Admin
        /// Query param: year (mặc định năm hiện tại)
        /// </remarks>
        [HttpGet("revenue/by-month")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueByMonth([FromQuery] int year = 0)
        {
            if (year <= 0) year = DateTime.UtcNow.Year;
            var result = await _dashboardService.GetRevenueByMonthAsync(year);
            return Ok(result);
        }

        // DOCTOR DASHBOARD
        [HttpGet("doctor/me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorDashboard()
        {
            var currentUserId = GetCurrentUserId();

            // Lấy DoctorId từ UserId hiện tại
            var doctor = await _context.Doctors
                .AsNoTracking()
                .Where(d => d.UserId == currentUserId)
                .Select(d => new { d.Id })
                .FirstOrDefaultAsync();

            if (doctor == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ bác sĩ cho tài khoản này" });

            var result = await _dashboardService.GetDoctorDashboardAsync(doctor.Id);
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
