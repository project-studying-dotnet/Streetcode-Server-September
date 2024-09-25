using MediatR;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.MediatR.Media.Art.Delete;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.MediatR.Media.Art.Create;
using Streetcode.BLL.MediatR.Media.Art.GetAll;
using Streetcode.BLL.MediatR.Media.Art.GetById;
using Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Microsoft.AspNetCore.Authorization;

namespace Streetcode.WebApi.Controllers.Media.Images;

public class ArtController : BaseApiController
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllArtsQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetArtByIdQuery(id)));
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetArtsByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Create([FromBody] ArtCreateDto artCreateDto)
    {
        return HandleResult(await Mediator.Send(new CreateArtCommand(artCreateDto)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id) 
    {
        return HandleResult(await Mediator.Send(new DeleteArtCommand(id)));
    }
}
