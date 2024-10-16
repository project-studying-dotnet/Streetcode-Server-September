namespace Streetcode.BLL.Dto.Streetcode.TextContent.Fact;

public class FactCreateDto
{
    public string Title { get; set; } = null!;
    public int? ImageId { get; set; }
    public string? FactContent { get; set; }
    public int StreetcodeId { get; set; }
}