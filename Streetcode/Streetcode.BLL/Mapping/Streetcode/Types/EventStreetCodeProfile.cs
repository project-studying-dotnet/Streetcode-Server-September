using AutoMapper;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Dto.Streetcode.Types;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.Types;

namespace Streetcode.BLL.Mapping.Streetcode.Types;

public class EventStreetcodeProfile : Profile
{
    public EventStreetcodeProfile()
    {
        CreateMap<EventStreetcode, EventStreetcodeDto>()
            .IncludeBase<StreetcodeContent, StreetcodeDto>().ReverseMap();
  }
}
