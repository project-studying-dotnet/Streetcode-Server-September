using Streetcode.BLL.Dto.AdditionalContent;

namespace Streetcode.BLL.Dto.Media.Video;

public class VideoDto
{
  public int Id { get; set; }
  public string? Description { get; set; }
  public string? Url { get; set; }
  public int StreetcodeId { get; set; }
}
