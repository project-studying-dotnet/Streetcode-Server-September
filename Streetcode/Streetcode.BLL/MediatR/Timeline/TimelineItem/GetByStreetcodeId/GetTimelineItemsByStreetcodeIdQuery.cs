using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Timeline;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;

public record GetTimelineItemsByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<TimelineItemDto>>>;