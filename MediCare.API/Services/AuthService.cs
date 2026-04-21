using AutoMapper;
using MediCare.API.Data;
using MediCare.API.DTOs;
using MediCare.API.Entities;
using MediCare.API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<long>> roleManager,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            AppDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _emailService = emailService;
        }

        // 1. ĐĂNG KÝ (REGISTER)
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Kiểm tra username
            if (await _userManager.FindByNameAsync(request.Username) != null)
                throw new BadHttpRequestException("Username đã được sử dụng");
            // kiểm tra email
            if(await _userManager.FindByEmailAsync(request.Email) != null)
                throw new BadHttpRequestException("Email đã được sử dụng");

            // map thủ công 
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

            return await GenerateJwtToken(user);
        }

        // 2. ĐĂNG NHẬP (LOGIN)
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new BadHttpRequestException("Tài khoản hoặc mật khẩu không chính xác");

            // kiểm tra trạng thái tài khoản
            if (!user.IsActive)
                throw new UnauthorizedAccessException("Tài khoản đã bị khóa");

            // lấy role
            var roles = await _userManager.GetRolesAsync(user);
            
            // token
            var accessToken = await GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync(); // lưu refresh token vào DB

            var expiresInSeconds = Convert.ToInt32(_configuration["Jwt:DurationInMinutes"]) * 60;

            return new LoginResponse
            {
                AccessToken = accessToken.AccessToken,
                RefreshToken = refreshToken,
                ExpiresIn = DateTime.UtcNow.AddSeconds(expiresInSeconds),
                User = new UserInfoResponse
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email ?? "",
                    Roles = roles
                }
            };
        }

        // REFRESH TOKEN
        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest refreshToken)
        {
            // tìm token trong DB
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken.Token);

            // kiểm tra trạng thái token
            if(storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
                throw new BadHttpRequestException("Refresh token không hợp lệ");

            var user = storedToken.User; // Lấy user từ thẻ
            var roles = await _userManager.GetRolesAsync(user);
            var authResponse = await GenerateJwtToken(user);
            var newRefreshTokenString = GenerateRefreshToken();

            // tạo refresh token mới
            storedToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedToken);
            
            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _context.RefreshTokens.Add(newRefreshToken);

            // Lưu xuống DB
            await _context.SaveChangesAsync();

            var expiresInSeconds = Convert.ToInt32(_configuration["Jwt:DurationInMinutes"]) * 60;

            return new LoginResponse
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = newRefreshTokenString,
                ExpiresIn = DateTime.UtcNow.AddSeconds(expiresInSeconds),
                User = new UserInfoResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email ?? "",
                    Roles = roles
                }
            };
        }

        // REVOKE TOKEN
        public async Task RevokeTokenAsync(RevokeTokenRequest revokeToken)
        {
            // tìm token trong db
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == revokeToken.Token);
            if (storedToken == null || storedToken.IsRevoked) return;

            storedToken.IsRevoked = true;
            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();
        }
        // FORGOT PASSWORD
        public async Task ForgotPasswordAsync(ForgotPasswordRequest forgotPassword)
        {
            // tìm user theo email
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email);
            
            if (user == null || !user.IsActive) return; // email không tồn tại return 

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Mã hoá token để gửi qua email
            var encodedToken = Uri.EscapeDataString(resetToken);
            var encodedEmail = Uri.EscapeDataString(user.Email!);

            var clientUrl = _configuration["App:ClientUrl"]
                ?? throw new InvalidOperationException("App:ClientUrl chưa được cấu hình");

            var resetLink = $"{clientUrl}/auth/reset-password?token={encodedToken}&email={encodedEmail}";

            try
            {
                await _emailService.SendPasswordResetAsync(
                    toEmail: user.Email!,
                    toName: user.FullName,
                    resetLink: resetLink);

                _logger.LogInformation(
                    "Đã gửi email đặt lại mật khẩu đến {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Gửi email đặt lại mật khẩu đến {Email} thất bại", user.Email);
            }

            // DEV
            _logger.LogInformation($"[DEV ONLY] Token reset pass cho {forgotPassword.Email} là: {resetToken}");

        }

        // RESET PASSWORD
        public async Task ResetPasswordAsync(ResetPasswordRequest resetPassword)
        {
            // kiểm tra mật khẩu
            if(resetPassword.NewPassword != resetPassword.ConfirmPassword)
                throw new BadHttpRequestException("Mật khẩu mới và xác nhận mật khẩu không khớp");

            // tìm user theo email
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if(user == null)
                throw new BadHttpRequestException("Yêu cầu không hợp lệ."); 

            // kiểm tra token
            var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.NewPassword);
            // return kết quả
            if (!result.Succeeded)
            {
                // 
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadHttpRequestException(errors);
            }
        }

        // CHANGE PASSWORD
        public async Task ChangePasswordAsync(long userId, ChangePasswordRequest request)
        {
            // tìm user
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new KeyNotFoundException("User không tồn tại");

            // kiểm tra mật khẩu cũ
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                // trả về lỗi chi tiết
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadHttpRequestException(errors);
            }
        }

        // LẤY THÔNG TIN USER HIỆN TẠI
        public async Task<UserInfoResponse> GetCurrentUserAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new KeyNotFoundException("User không tồn tại");

            var roles = await _userManager.GetRolesAsync(user);
            var response = _mapper.Map<UserInfoResponse>(user);
            response.Roles = roles;

            return response;
        }

        // HELPER: TẠO JWT TOKEN
        private async Task<AuthResponse> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("FullName", user.FullName),
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
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            };
        }
        // 
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

    }
}