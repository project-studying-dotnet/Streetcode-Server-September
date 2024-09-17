using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Update
{
    public record CreateTimelineItemCommand(TimelineItemUpdateDto timelineItemUpdateDto) : IRequest<Result<TimelineItemDto>>;
}
