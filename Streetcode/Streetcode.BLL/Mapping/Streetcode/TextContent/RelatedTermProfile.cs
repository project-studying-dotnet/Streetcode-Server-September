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
        // CreateMap<RelatedTerm, RelatedTermFullDto>()
        //     .ForPath(dto => dto.Id, config => config.MapFrom(entity => entity.Id))
        //     .ForPath(dto => dto.Word, config => config.MapFrom(entity => entity.Word))
        //     .ForPath(dto => dto.TermDto, config => config.MapFrom(entity => entity.Term));
        // CreateMap<RelatedTerm, RelatedTermFullDto>()
        //     .ForMember(dto => dto.Id, config => config.MapFrom(entity => entity.Id))
        //     .ForMember(dto => dto.Word, config => config.MapFrom(entity => entity.Word))
        //     .ForMember(dto => dto.TermDto, config => config.MapFrom(entity => entity.Term));
        CreateMap<RelatedTerm, RelatedTermFullDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Word", opt => opt.MapFrom(src => src.Word))
            .ForCtorParam("TermDto", opt => opt.MapFrom(src => src.Term));
    }
}
