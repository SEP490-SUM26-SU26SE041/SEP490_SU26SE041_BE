using System;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmSEP490.Model.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ProfileDescription { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public string? Phone { get; set; }
        
        public string? ProfileDescription { get; set; }

        public string Role { get; set; } = "Student";
    }

    public class UpdateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Phone { get; set; }
        
        public string? ProfileDescription { get; set; }

        public string Role { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
    }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
