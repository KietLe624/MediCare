using static MediCare.API.DTOs.AuthDTO;

namespace MediCare.API.Services
{
    public interface IAuthService
    {  
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        //Task<AuthResponse> LoginAsync(LoginRequest request);
        //Task<RefreshToken> RefreshTokenAsync(RefreshToken request);
        //Task RevokeTokenAsync(RevokeTokenRequest request);
        //Task ForgotPasswordAsync(ForgotPasswordRequest request);
        //Task ResetPasswordAsync(ResetPasswordRequest request);
        //Task ChangePasswordAsync(long userId, ChangePasswordRequest request);
        Task<UserInfoResponse> GetCurrentUserAsync(long userId);
    }

}


