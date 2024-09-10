using Streetcode.BLL.Dto.AdditionalContent;

namespace Streetcode.BLL.Dto.Media.Audio;

public class AudioDto
{
  public int Id { get; set; }
  public string? Description { get; set; }
  public string BlobName { get; set; }
  public string Base64 { get; set; }
  public string MimeType { get; set; }
}