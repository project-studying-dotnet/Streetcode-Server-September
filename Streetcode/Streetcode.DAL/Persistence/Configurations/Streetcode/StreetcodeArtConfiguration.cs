namespace Streetcode.DAL.Persistence.Configurations.Streetcode;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.Streetcode;

public class StreetcodeArtConfiguration: IEntityTypeConfiguration<StreetcodeArt>
{
    public void Configure(EntityTypeBuilder<StreetcodeArt> builder)
    {
        builder.HasKey(d => new { d.ArtId, d.StreetcodeId });

        builder.HasOne(d => d.Streetcode)
            .WithMany(d => d.StreetcodeArts)
            .HasForeignKey(d => d.StreetcodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Art)
            .WithMany(d => d.StreetcodeArts)
            .HasForeignKey(d => d.ArtId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.Index)
            .HasDefaultValue(1);

        builder.HasIndex(d => new { d.ArtId, d.StreetcodeId })
            .IsUnique(false);
    }
}