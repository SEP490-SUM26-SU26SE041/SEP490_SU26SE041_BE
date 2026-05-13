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
                new User { Id = 1, FullName = "System Admin", Email = "admin@farm.com", PasswordHash = "admin123", Role = "Admin" }
            );
        }
    }
}
