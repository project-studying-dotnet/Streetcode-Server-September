using AutoMapper;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.DAL.Entities.Timeline;

namespace Streetcode.BLL.Mapping.Timeline;

public class TimelineItemProfile : Profile
{
    public TimelineItemProfile()
    {
        CreateMap<TimelineItem, TimelineItemDto>().ReverseMap();

        CreateMap<TimelineItem, TimelineItemUpdateDto>().ReverseMap();

        CreateMap<TimelineItem, TimelineItemUpdateDto>()
            .ForMember(dest => dest.HistoricalContexts, opt => opt.MapFrom(x => x.HistoricalContextTimelines
                .Select(hct => new HistoricalContextDto
                {
                    Id = hct.HistoricalContextId,
                    Title = hct.HistoricalContext!.Title ?? string.Empty
                }).ToList()))
            .ReverseMap();

        CreateMap<TimelineItem, TimelineItemCreateDto>().ReverseMap();
        
    }
}
