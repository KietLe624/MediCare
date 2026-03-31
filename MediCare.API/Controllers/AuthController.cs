using MediCare.API.Entities;
using MediCare.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static MediCare.API.DTOs.AuthDTO;


namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username))
                return BadRequest("Dữ liệu từ Postman gửi lên bị rỗng!");
            
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
    }
}
