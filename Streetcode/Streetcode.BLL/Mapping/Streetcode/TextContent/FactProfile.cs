using AutoMapper;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.DAL.Entities.Streetcode.TextContent;

namespace Streetcode.BLL.Mapping.Streetcode.TextContent;

public class FactProfile : Profile
{
    public FactProfile()
    {
        CreateMap<Fact, FactDto>().ReverseMap();
        CreateMap<Fact, FactUpdateCreateDto>().ReverseMap();
        CreateMap<Fact, FactCreateDto>().ReverseMap();
        CreateMap<Fact, FactUpdateDto>().ReverseMap();
        CreateMap<Fact, FactOrderUpdateDto>().ReverseMap();
    }
}
