using AutoMapper;
using Streetcode.BLL.Dto.Analytics;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Analytics;

namespace Streetcode.BLL.Mapping.Analytics;

public class StatisticRecordProfile: Profile
{
    public StatisticRecordProfile() 
    {
        CreateMap<StatisticRecord, StatisticRecordDto>().ReverseMap();
        CreateMap<StatisticRecordCreateDto, StatisticRecord>()
           .ForMember(dest => dest.StreetcodeId, opt => opt.MapFrom(src => src.StreetcodeCoordinate.StreetcodeId));
    }    
}
