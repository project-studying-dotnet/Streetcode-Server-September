namespace Streetcode.BLL.Dto.Streetcode.TextContent.Text;

public class TextCreateDto
{
    public string Title { get; set; } = null!;
    public string TextContent { get; set; } = null!;
    public string? AdditionalText { get; set; }
    public int StreetcodeId { get; set; }
}