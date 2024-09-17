using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;


namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public record CreateTimelineItemCommand(TimelineItemCreateDto timelineItemCreateDto): IRequest<Result<TimelineItemDto>>;
}
