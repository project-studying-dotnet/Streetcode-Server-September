namespace Streetcode.DAL.Persistence.Configurations.Partners;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.Partners;

public class PartnerConfiguration: IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.HasMany(d => d.PartnerSourceLinks)
            .WithOne(p => p.Partner)
            .HasForeignKey(d => d.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.IsKeyPartner)
            .HasDefaultValue("false");
    }
}