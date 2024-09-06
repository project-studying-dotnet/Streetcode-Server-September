using AutoMapper;
using Streetcode.BLL.Dto.AdditionalContent.Subtitles;
using Streetcode.DAL.Entities.AdditionalContent;

namespace Streetcode.BLL.Mapping.AdditionalContent;

public class SubtitleProfile : Profile
{
   public SubtitleProfile()
   {
        CreateMap<Subtitle, SubtitleDto>().ReverseMap();
  }
}
