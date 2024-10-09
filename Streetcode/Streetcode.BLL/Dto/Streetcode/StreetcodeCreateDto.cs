using Streetcode.BLL.Dto.AdditionalContent.Tag;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.Dto.Streetcode;

public class StreetcodeCreateDto
{
    public int Index { get; set; }

    public StreetcodeType StreetcodeType { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? Rank { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [Required]
    public DateTime EventStartOrPersonBirthDate { get; set; }

    public DateTime? EventEndOrPersonDeathDate { get; set; }

    public IReadOnlyList<TagShortDto>? Tags { get; init; }

    [MaxLength(450)]
    public string? Teaser { get; set; }

    [Required]
    [MaxLength(150)]
    public string TransliterationUrl { get; set; } = null!;
    
    [MaxLength(33)]
    public string? BriefDescription { get; set; }

    public AudioDto? AudioDto { get; set; }

    public ImageDto BlackAndWhiteImageDto { get; set; } = null!;

    public ImageDto? HistoryLinksImageDto { get; set; }

    public ImageDto? GifDto { get; set; }
}
