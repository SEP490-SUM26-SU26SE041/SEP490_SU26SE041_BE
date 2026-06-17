using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmSEP490.Model.DTOs;

namespace SmartFarmSEP490.Service.Interfaces.Auth
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> CreateUserAsync(CreateUserDto request);
        Task<bool> UpdateUserAsync(Guid id, UpdateUserDto request);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> ToggleUserStatusAsync(Guid id);
        
        Task<List<RoleDto>> GetAllRolesAsync();
    }
}
