using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.Dto.Partners;

public class PartnerDto
{
    public int Id { get; set; }
    public bool IsKeyPartner { get; set; }
    public bool IsVisibleEverywhere { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int LogoId { get; set; }
    public UrlDto? TargetUrl { get; set; }
    public List<PartnerSourceLinkDto>? PartnerSourceLinks { get; set; }
    public List<StreetcodeShortDto>? Streetcodes { get; set; }
}