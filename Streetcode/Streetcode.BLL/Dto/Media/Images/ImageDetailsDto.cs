using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.Dto.Media.Images
{
    public class ImageDetailsDto
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(200)]
        public string? Alt { get; set; }

        public int ImageId { get; set; }
    }
}
