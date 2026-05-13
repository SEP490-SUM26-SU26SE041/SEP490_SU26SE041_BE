using SmartFarmSEP490.Model;

namespace SmartFarmSEP490.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
    }
}
