namespace Streetcode.BLL.Dto.AdditionalContent.Subtitles;

public class SubtitleDto
{
    public int Id { get; set; }
    public string SubtitleText { get; set; } = null!;
    public int StreetcodeId { get; set; }
}
