using AutoMapper;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.DAL.Entities.Streetcode.TextContent;

namespace Streetcode.BLL.Mapping.Streetcode.TextContent;

public class RelatedTermProfile : Profile
{
    public RelatedTermProfile()
    {
        CreateMap<RelatedTerm, RelatedTermDto>().ReverseMap();
        CreateMap<RelatedTermCreateDto, RelatedTerm>();
        CreateMap<RelatedTerm, RelatedTermFullDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Word", opt => opt.MapFrom(src => src.Word))
            .ForCtorParam("TermDto", opt => opt.MapFrom(src => src.Term));
    }
}
