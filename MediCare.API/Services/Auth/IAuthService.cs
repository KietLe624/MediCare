using static MediCare.API.DTOs.AuthDTO;

namespace MediCare.API.Services.Auth
{
    public interface IAuthService
    {  
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request );
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest refreshToken);  
        Task RevokeTokenAsync(RevokeTokenRequest revokeToken);
        Task ForgotPasswordAsync(ForgotPasswordRequest forgotPassword);
        Task ResetPasswordAsync(ResetPasswordRequest resetPassword);
        Task ChangePasswordAsync(long userId, ChangePasswordRequest request);
        Task<UserInfoResponse> GetCurrentUserAsync(long userId);
    }

}


