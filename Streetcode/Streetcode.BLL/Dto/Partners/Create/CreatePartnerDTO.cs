using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Dto.Partners.Create;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.Dto.Partners
{
    public class CreatePartnerDto
    {
        public int Id { get; set; }
        public bool IsKeyPartner { get; set; }
        public bool IsVisibleEverywhere { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? TargetUrl { get; set; }
        public int LogoId { get; set; }
        public string? UrlTitle { get; set; }
        public List<CreatePartnerSourceLinkDto>? PartnerSourceLinks { get; set; }
        public List<StreetcodeShortDto> Streetcodes { get; set; } = new List<StreetcodeShortDto>();
    }
}
