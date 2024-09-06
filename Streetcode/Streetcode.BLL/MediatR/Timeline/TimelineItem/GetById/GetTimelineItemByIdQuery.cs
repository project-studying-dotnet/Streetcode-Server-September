using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;

public record GetTimelineItemByIdQuery(int Id) : IRequest<Result<TimelineItemDto>>;
