using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<LoginResponse?> LoginWithGoogleAsync(GoogleLoginRequest request);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> VerifyResetCodeAsync(string email, string code);
        Task<bool> ResetPasswordAsync(string email, string code, string newPassword);
    }
}
