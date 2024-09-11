namespace Streetcode.DAL.Persistence.Configurations.AdditionalContent;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.AdditionalContent;

public class TagConfiguration: IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasMany(t => t.Streetcodes)
            .WithMany(s => s.Tags)
            .UsingEntity<StreetcodeTagIndex>(
                sp => sp
                    .HasOne(x => x.Streetcode)
                    .WithMany(x => x.StreetcodeTagIndices)
                    .HasForeignKey(x => x.StreetcodeId),
                sp => sp.HasOne(x => x.Tag).WithMany(x => x.StreetcodeTagIndices).HasForeignKey(x => x.TagId));
    }
}