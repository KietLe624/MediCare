using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class AuthDTO
    {
        public class RegisterRequest
        {
            [Required]
            public string Username { get; set; } = string.Empty;

            [Required]
            public string FullName { get; set; } = null!;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;

            public string? PhoneNumber { get; set; }
            public string Role { get; set; } = "Patient"; // mặc định là Patient
        }
        public class LoginRequest
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime ExpiresIn { get; set; }
            public UserInfoResponse User { get; set; } = null!;
        }
        public class RefreshTokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }
        public class RevokeTokenRequest
        {
            [Required]
            public string Token { get; set; } = string.Empty;
        }
        public class ForgotPasswordRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
        }
         public class ResetPasswordRequest
         {
            [Required, EmailAddress]
            public string? Email { get; set; }
            [Required]
            public string Token { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
         }
        public class ChangePasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;
        }
        //public class AuthResponse
        //{
        //    public string AccessToken { get; set; } = string.Empty;
        //    public string RefreshToken { get; set; } = string.Empty;
        //    public DateTime AccessTokenExpiresAt { get; set; }
        //    public DateTime RefreshTokenExpiresAt { get; set; }
        //}
        public class AuthResponse
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string AccessToken { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
            public IList<string> Roles { get; set; } = new List<string>();
        }
        public class UserInfoResponse
        {
            public long Id { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string FullName { get; set; } = null!;
            public string Email { get; set; } = string.Empty;
            public string? PhoneNumber { get; set; }
            public IList<string> Roles { get; set; } = new List<string>();
        }

    }
}
