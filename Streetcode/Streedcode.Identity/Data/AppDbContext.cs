using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Streetcode.Identity.Models;

namespace Streetcode.Identity.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
        
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
              .HasMany(u => u.RefreshTokens) 
                .WithOne(rt => rt.User) 
                .HasForeignKey(rt => rt.UserId) 
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
