using SmartFarmSEP490.Model;
using Task = System.Threading.Tasks.Task;
namespace SmartFarmSEP490.Repository.Interfaces.Auth
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<List<User>> GetAllUsersAsync();
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<Role?> GetRoleByIdAsync(Guid id);
        Task<List<Role>> GetAllRolesAsync();
        Task<User> AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
    }
}
