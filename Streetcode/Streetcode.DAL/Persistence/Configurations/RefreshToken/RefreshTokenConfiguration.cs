using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.DAL.Persistence.Configurations.Refresh
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasOne(d => d.User)
                .WithOne(u => u.RefreshToken)
                .HasForeignKey<User>(u => u.RefreshTokenId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
