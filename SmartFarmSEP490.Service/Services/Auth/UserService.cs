using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmSEP490.Model;
using SmartFarmSEP490.Model.DTOs;
using SmartFarmSEP490.Repository.Interfaces.Auth;
using SmartFarmSEP490.Service.Interfaces.Auth;

namespace SmartFarmSEP490.Service.Services.Auth
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                ProfileDescription = u.ProfileDescription,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Role = u.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Student"
            }).ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var u = await _userRepository.GetUserByIdAsync(id);
            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                ProfileDescription = u.ProfileDescription,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Role = u.UserRoles.FirstOrDefault()?.Role?.RoleName ?? "Student"
            };
        }

        public async Task<UserDto?> CreateUserAsync(CreateUserDto request)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null) return null; // Email already exists

            var role = await _userRepository.GetRoleByNameAsync(request.Role);
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                ProfileDescription = request.ProfileDescription,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (role != null)
            {
                user.UserRoles.Add(new UserRole { RoleId = role.Id, AssignedAt = DateTime.UtcNow });
            }

            var createdUser = await _userRepository.AddUserAsync(user);

            return new UserDto
            {
                Id = createdUser.Id,
                FullName = createdUser.FullName,
                Email = createdUser.Email,
                Phone = createdUser.Phone,
                ProfileDescription = createdUser.ProfileDescription,
                IsActive = createdUser.IsActive,
                CreatedAt = createdUser.CreatedAt,
                Role = role?.RoleName ?? "Student"
            };
        }

        public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto request)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;

            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.ProfileDescription = request.ProfileDescription;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var currentRole = user.UserRoles.FirstOrDefault();
            if (currentRole?.Role?.RoleName != request.Role)
            {
                var newRole = await _userRepository.GetRoleByNameAsync(request.Role);
                if (newRole != null)
                {
                    user.UserRoles.Clear();
                    user.UserRoles.Add(new UserRole { RoleId = newRole.Id, AssignedAt = DateTime.UtcNow });
                }
            }

            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> ToggleUserStatusAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;

            // Soft delete
            user.DeletedAt = DateTime.UtcNow;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            return true;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _userRepository.GetAllRolesAsync();
            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                RoleName = r.RoleName,
                Description = r.Description
            }).ToList();
        }
    }
}
