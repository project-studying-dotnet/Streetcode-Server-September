namespace Streetcode.BLL.Dto.Media.Video;

public class VideoCreateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string Url { get; set; } = null!;
    public int StreetcodeId { get; set; }
}