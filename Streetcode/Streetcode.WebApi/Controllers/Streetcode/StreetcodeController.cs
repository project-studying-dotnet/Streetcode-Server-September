using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAll;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetById;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByTransliterationUrl;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAllShort;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAllCatalog;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetCount;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByFilter;
using Streetcode.BLL.Dto.AdditionalContent.Filter;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetShortById;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAllStreetcodesMainPage;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.DeleteHard;
using Microsoft.AspNetCore.Authorization;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.ExistWithUrl;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.AddToFavorite;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetFavouritesList;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.RemoveFromFavourites;


namespace Streetcode.WebApi.Controllers.Streetcode;

public class StreetcodeController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllStreetcodesRequestDto request)
    {
        return HandleResult(await Mediator.Send(new GetAllStreetcodesQuery(request)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllShort()
    {
        return HandleResult(await Mediator.Send(new GetAllStreetcodesShortQuery()));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMainPage()
    {
        return HandleResult(await Mediator.Send(new GetAllStreetcodesMainPageQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShortById(int id)
    {
        return HandleResult(await Mediator.Send(new GetStreetcodeShortByIdQuery(id)));
    }

    [HttpGet]
    public async Task<IActionResult> GetByFilter([FromQuery] StreetcodeFilterRequestDto request)
    {
        return HandleResult(await Mediator.Send(new GetStreetcodeByFilterQuery(request)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCatalog([FromQuery] int page, [FromQuery] int count)
    {
        return HandleResult(await Mediator.Send(new GetAllStreetcodesCatalogQuery(page, count)));
    }

    [HttpGet]
    public async Task<IActionResult> GetCount()
    {
        return HandleResult(await Mediator.Send(new GetStreetcodesCountQuery()));
    }

    [HttpGet("{url}")]
    public async Task<IActionResult> GetByTransliterationUrl([FromRoute] string url)
    {
        return HandleResult(await Mediator.Send(new GetStreetcodeByTransliterationUrlQuery(url)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetStreetcodeByIdQuery(id)));
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] StreetcodeMainBlockCreateDto streetcodeMainBlockCreateDto)
    {
        return HandleResult(await Mediator.Send(new CreateStreetcodeMainBlockCommand(streetcodeMainBlockCreateDto)));
    }

    [HttpGet("{url}")]
    public async Task<IActionResult> ExistWithUrl([FromRoute] string url)
    {
        return HandleResult(await Mediator.Send(new ExistWithUrlQuery(url)));
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToFavourites([FromBody] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new AddStreetcodeToFavouritesCommand(streetcodeId)));
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetFavouritesList()
    {
        return HandleResult(await Mediator.Send(new GetFavouritesListQuery()));
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromFavourites([FromBody] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new RemoveStreetcodeFromFavouritesCommand(streetcodeId)));
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteHardStreetcode(int id)
    {
        return HandleResult(await Mediator.Send(new DeleteHardStreetcodeCommand(id)));  
    } 
}
