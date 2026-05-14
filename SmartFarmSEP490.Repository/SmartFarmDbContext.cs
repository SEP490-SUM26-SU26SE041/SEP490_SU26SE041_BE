using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;
using BCrypt.Net;

namespace SmartFarmSEP490.Repository
{
    public class SmartFarmDbContext : DbContext
    {
        public SmartFarmDbContext(DbContextOptions<SmartFarmDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Seed some data for testing if needed
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "System Admin", Email = "admin@farm.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin" },
                new User { Id = 2, FullName = "Farm Manager", Email = "manager@farm.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Manager" },
                new User { Id = 3, FullName = "Technician", Email = "tech@farm.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Technician" },
                new User { Id = 4, FullName = "Researcher", Email = "researcher@farm.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Researcher" },
                new User { Id = 5, FullName = "Student", Email = "student@farm.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Student" }
            );
        }
    }
}
