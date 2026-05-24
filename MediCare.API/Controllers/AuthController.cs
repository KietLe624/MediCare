using Azure.Core;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName))
                return BadRequest("Dữ liệu từ Postman gửi lên bị rỗng!");
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshToken)
        {
            // kiểm tra req có rộng không
            if (refreshToken == null || string.IsNullOrEmpty(refreshToken.Token))
                return BadRequest("Dữ liệu từ Postman gửi lên bị rỗng!");
            var response = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest revokeToken)
        {
            if (revokeToken == null || string.IsNullOrEmpty(revokeToken.Token))
                return BadRequest("Thiếu dữ liệu");
            await _authService.RevokeTokenAsync(revokeToken);
            return Ok(new { message = "Token đã được thu hồi" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest forgotPassword)
        {
            if (forgotPassword == null || string.IsNullOrEmpty(forgotPassword.Email))
                return BadRequest("Thiếu dữ liệu");
            await _authService.ForgotPasswordAsync(forgotPassword);
            return Ok(new { message = "Liên kết đặt lại mật khẩu đã được gửi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPassword)
        {
            if (resetPassword == null || string.IsNullOrEmpty(resetPassword.Email) || string.IsNullOrEmpty(resetPassword.Token))
                return BadRequest("Thiếu dữ liệu");
            await _authService.ResetPasswordAsync(resetPassword);
            return Ok(new { message = "Mật khẩu đã được đặt lại thành công." });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromHeader] long userId, [FromBody] ChangePasswordRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("Thiếu dữ liệu");
            await _authService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Mật khẩu đã được thay đổi thành công." });
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var currentUserId = GetCurrentUserId();

            var userInfo = await _authService.GetCurrentUserAsync(currentUserId);
            return Ok(userInfo);
        }

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
