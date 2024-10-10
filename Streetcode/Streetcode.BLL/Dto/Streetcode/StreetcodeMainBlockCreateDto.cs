using Streetcode.BLL.Dto.AdditionalContent.Tag;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.Dto.Streetcode;

public class StreetcodeMainBlockCreateDto
{
    public int Index { get; set; }

    public StreetcodeType StreetcodeType { get; set; }
    
    public string? Title { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? Rank { get; set; }

    public string? LastName { get; set; }

    public DateTime EventStartOrPersonBirthDate { get; set; }

    public DateTime? EventEndOrPersonDeathDate { get; set; }

    public List<TagShortDto>? Tags { get; set; }

    public string? Teaser { get; set; }
    
    public string TransliterationUrl { get; set; } = null!;
    
    public string? BriefDescription { get; set; }

    public AudioFileBaseCreateDto? AudioFileBaseCreate { get; set; }

    public ImageFileBaseCreateDto BlackAndWhiteImageFileBaseCreateDto { get; set; } = null!;

    public ImageFileBaseCreateDto? HistoryLinksImageFileBaseCreateDto { get; set; }

    public ImageFileBaseCreateDto? GifFileBaseCreateDto { get; set; }
}
