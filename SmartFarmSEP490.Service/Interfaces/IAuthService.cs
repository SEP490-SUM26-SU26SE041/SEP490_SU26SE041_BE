using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
    }
}
