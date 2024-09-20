using AutoMapper;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.DAL.Entities.Streetcode.TextContent;

namespace Streetcode.BLL.Mapping.Streetcode.TextContent;

public class TermProfile : Profile
{
    public TermProfile()
    {
        CreateMap<Term, TermDto>().ReverseMap();
        CreateMap<Term, TermCreateDto>().ReverseMap();
    }
}