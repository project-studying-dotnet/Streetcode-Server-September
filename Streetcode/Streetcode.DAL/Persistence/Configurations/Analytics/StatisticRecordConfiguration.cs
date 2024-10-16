namespace Streetcode.DAL.Persistence.Configurations.Analytics;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.Analytics;

public class StatisticRecordConfiguration: IEntityTypeConfiguration<StatisticRecord>
{
    public void Configure(EntityTypeBuilder<StatisticRecord> builder)
    {
        builder.HasOne(x => x.StreetcodeCoordinate)
            .WithOne(x => x.StatisticRecord)
            .HasForeignKey<StatisticRecord>(x => x.StreetcodeCoordinateId);
    }
}