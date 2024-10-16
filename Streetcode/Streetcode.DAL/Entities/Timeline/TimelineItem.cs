using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;

namespace Streetcode.DAL.Entities.Timeline;

[Table("timeline_items", Schema = "timeline")]
public class TimelineItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public DateViewPattern DateViewPattern { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int StreetcodeId { get; set; }

    public StreetcodeContent? Streetcode { get; set; }

    public List<HistoricalContextTimeline> HistoricalContextTimelines { get; set; } = new ();
}
