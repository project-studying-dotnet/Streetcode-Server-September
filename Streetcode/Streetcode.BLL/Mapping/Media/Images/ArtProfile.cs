using AutoMapper;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.DAL.Entities.Media.Images;

namespace Streetcode.BLL.Mapping.Media.Images;

public class ArtProfile : Profile
{
    public ArtProfile()
    {
        CreateMap<Art, ArtDto>().ReverseMap();
        CreateMap<ArtCreateDto, Art>();
        CreateMap<Art, ArtCreateDto>()
            .ForMember(dest => dest.Streetcodes, opt => opt.MapFrom(src => src.StreetcodeArts
            .Select(sc => new StreetcodeShortDto
            {
                Id = sc.StreetcodeId,
                Title = sc.Streetcode.Title
            }).ToList()));
    }
}