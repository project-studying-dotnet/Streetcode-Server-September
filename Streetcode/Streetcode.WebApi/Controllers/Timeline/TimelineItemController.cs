using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;

namespace Streetcode.WebApi.Controllers.Timeline;

public class TimelineItemController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllTimelineItemsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetTimelineItemByIdQuery(id)));
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetTimelineItemsByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteTimelineItemCommand(id)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TimelineItemCreateDto timelineItemCreateDto) 
    {
        return HandleResult(await Mediator.Send(new CreateTimelineItemCommand(timelineItemCreateDto)));
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] TimelineItemUpdateDto timelineItemUpdateDto)
    {
        return HandleResult(await Mediator.Send(new UpdateTimelineItemCommand(timelineItemUpdateDto)));
    }
}