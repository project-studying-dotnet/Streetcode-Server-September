using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;

public record GetAllTimelineItemsQuery : IRequest<Result<IEnumerable<TimelineItemDto>>>;