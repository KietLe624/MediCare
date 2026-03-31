using AutoMapper;
using MediCare.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static MediCare.API.DTOs.AuthDTO;

namespace MediCare.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<long>> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<long>> roleManager,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        // 1. ĐĂNG KÝ (REGISTER)
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new BadHttpRequestException("Username is required");

            // Kiểm tra tồn tại
            if (await _userManager.FindByNameAsync(request.Username) != null)
                throw new BadHttpRequestException("Username đã được sử dụng");

            // mapp thủ công
            var user = new ApplicationUser
            {
                UserName = request.Username, 
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadHttpRequestException(errors);
            }

            // Xử lý Role
            var role = string.IsNullOrEmpty(request.Role) ? "Patient" : request.Role;
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(role));
            }
            await _userManager.AddToRoleAsync(user, role);

            return await GenerateAuthResponseAsync(user);
        }

        // 2. ĐĂNG NHẬP (LOGIN)
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new BadHttpRequestException("Tài khoản hoặc mật khẩu không chính xác");

            return await GenerateAuthResponseAsync(user);
        }

        // 3. LẤY THÔNG TIN USER HIỆN TẠI
        public async Task<UserInfoResponse> GetCurrentUserAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new KeyNotFoundException("User không tồn tại");

            var roles = await _userManager.GetRolesAsync(user);
            var response = _mapper.Map<UserInfoResponse>(user);
            response.Roles = roles;

            return response;
        }

        // --- HELPER: TẠO JWT TOKEN THẬT ---
        private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("FullName", user.FullName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new AuthResponse
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };
        }
    }
}