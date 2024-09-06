using Streetcode.BLL.Dto.Media.Images;

namespace Streetcode.BLL.Dto.Media.Art;

public class ArtDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public int ImageId { get; set; }
    public ImageDto? Image { get; set; }
}
