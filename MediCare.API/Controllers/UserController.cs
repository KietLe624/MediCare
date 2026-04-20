using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Toàn bộ module chỉ Admin truy cập
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DTOs.UserDTO.UserQueryParams query)
        {
            var result = await _userService.GetAllAsync(query);
            return Ok(result);
        }
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _userService.GetByIdAsync(id);
            return Ok(result);
        }
        [HttpPatch("{id:long}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] DTOs.UserDTO.UpdateProfileRequest request)
        {
            var result = await _userService.UpdateAsync(id, request);
            return Ok(result);
        }
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
        [HttpPatch("{id:long}/role")]
        public async Task<IActionResult> UpdateUserRoles(long id, [FromBody] DTOs.UserDTO.UpdateUserRoleRequest request)
        {
            var result = await _userService.UpdateRoleAsync(id, request);
            return Ok(result);
        }
        [HttpPatch("{id:long}/activate")]
        public async Task<IActionResult> UpdateUserActivate(long id, [FromBody] DTOs.UserDTO.UpdateUserStatusRequest request)
        {
            var result = await _userService.UpdateActivateAsync(id, request);
            return Ok(new
            {
                Message = "Cập nhật trạng thái người dùng thành công.",
                Data = result
            });
        }
    }
}
