using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MediCare.API.DTOs
{
    public class AuthDTO
    {
        public class RegisterRequest
        {
            [Required]
            [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Username must contain only letters or digits.")]
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
        public class RefreshToken 
        {
            public string Token { get; set; } = string.Empty;
        }
        public class RevokeTokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }
        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }
         public class ResetPasswordRequest
         {
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
            public string Token { get; set; } = string.Empty;
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
