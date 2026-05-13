using Microsoft.EntityFrameworkCore;
using SmartFarmSEP490.Model;

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
                new User { Id = 1, FullName = "System Admin", Email = "admin@farm.com", PasswordHash = "admin123", Role = "Admin" },
                new User { Id = 2, FullName = "Farm Manager", Email = "manager@farm.com", PasswordHash = "manager123", Role = "Manager" },
                new User { Id = 3, FullName = "Technician", Email = "tech@farm.com", PasswordHash = "tech123", Role = "Technician" },
                new User { Id = 4, FullName = "Researcher", Email = "researcher@farm.com", PasswordHash = "researcher123", Role = "Researcher" },
                new User { Id = 5, FullName = "Student", Email = "student@farm.com", PasswordHash = "student123", Role = "Student" }
            );
        }
    }
}
