using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;
namespace SmartFarmSEP490.Repository.Interfaces.Auth
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}
