using AutoMapper;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Dto.Partners.Create;
using Streetcode.DAL.Entities.Partners;

namespace Streetcode.BLL.Mapping.Partners;

public class PartnerSourceLinkProfile : Profile
{
    public PartnerSourceLinkProfile()
    {
        CreateMap<PartnerSourceLink, PartnerSourceLinkDto>()
            .ForPath(dto => dto.TargetUrl.Href, conf => conf.MapFrom(ol => ol.TargetUrl));
        CreateMap<PartnerSourceLink, CreatePartnerSourceLinkDto>().ReverseMap();
    }
}
