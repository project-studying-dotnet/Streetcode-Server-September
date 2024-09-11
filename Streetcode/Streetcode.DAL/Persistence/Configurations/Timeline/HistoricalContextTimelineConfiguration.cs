namespace Streetcode.DAL.Persistence.Configurations.Timeline;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Entities.Timeline;

public class HistoricalContextTimelineConfiguration: IEntityTypeConfiguration<HistoricalContextTimeline>
{
    public void Configure(EntityTypeBuilder<HistoricalContextTimeline> builder)
    {
        builder.HasKey(ht => new { ht.TimelineId, ht.HistoricalContextId });
        
        builder.HasOne(ht => ht.Timeline)
            .WithMany(x => x.HistoricalContextTimelines)
            .HasForeignKey(x => x.TimelineId);
        
        builder.HasOne(ht => ht.HistoricalContext)
            .WithMany(x => x.HistoricalContextTimelines)
            .HasForeignKey(x => x.HistoricalContextId);
    }
}