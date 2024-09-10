using Streetcode.BLL.Dto.AdditionalContent;

namespace Streetcode.BLL.Dto.Partners;

public class PartnerSourceLinkDto
{
    public int Id { get; set; }
    public LogoTypeDto LogoType { get; set; }
    public UrlDto TargetUrl { get; set; }
}