namespace Streetcode.BLL.Dto.Media;

public class FileBaseCreateDto
{
    public string Title { get; set; } = null!;
    public string BaseFormat { get; set; } = null!;
    public string? MimeType { get; set; }
    public string Extension { get; set; } = null!;
}
