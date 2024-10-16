namespace Streetcode.BLL.Dto.Streetcode.TextContent.Text;

public class TextDto
{
  public int Id { get; set; }
  public string Title { get; set; } = null!;
  public string TextContent { get; set; } = null!;
  public int StreetcodeId { get; set; }
  public string? AdditionalText { get; set; }
}
