namespace Streetcode.DAL.Persistence.Configurations.AdditionalContent;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.AdditionalContent;

public class StreetcodeTagIndexConfiguration: IEntityTypeConfiguration<StreetcodeTagIndex>
{
    public void Configure(EntityTypeBuilder<StreetcodeTagIndex> builder)
    {
        builder.HasKey(nameof(StreetcodeTagIndex.StreetcodeId), nameof(StreetcodeTagIndex.TagId));
    }
}