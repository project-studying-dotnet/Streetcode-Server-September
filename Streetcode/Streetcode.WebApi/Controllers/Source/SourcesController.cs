using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoryById;
using Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoriesByStreetcodeId;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Delete;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;

namespace Streetcode.WebApi.Controllers.Source;

public class SourcesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAllNames()
    {
        return HandleResult(await Mediator.Send(new GetAllCategoryNamesQuery()));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        return HandleResult(await Mediator.Send(new GetAllCategoriesQuery()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetCategoryByIdQuery(id)));
    }

    [HttpGet("{categoryId:int}&{streetcodeId:int}")]
    public async Task<IActionResult> GetCategoryContentByStreetcodeId([FromRoute] int streetcodeId, [FromRoute] int categoryId)
    {
        return HandleResult(await Mediator.Send(new GetCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId)));
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetCategoriesByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetCategoriesByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SourceLinkCategoryCreateDto srcLinkCategoryCreateDto)
    {
        return HandleResult(await Mediator.Send(new CreateSourceLinkCategoryCommand(srcLinkCategoryCreateDto)));
    }

    [HttpPut("{categoryId:int}&{streetcodeId:int}")]
    public async Task<IActionResult> Update([FromRoute] int streetcodeId, [FromRoute] int categoryId, 
                                            [FromBody] SourceLinkCategoryContentUpdateDto srcLinkCategoryCreateDto)
    {
        return HandleResult(await Mediator.Send(new UpdateStreetcodeCategoryContentCommand(streetcodeId, categoryId, srcLinkCategoryCreateDto)));
    }

    [HttpDelete("{categoryId:int}&{streetcodeId:int}")]
    public async Task<IActionResult> DeleteCategoryContent([FromRoute] int streetcodeId, [FromRoute] int categoryId)
    {
        return HandleResult(await Mediator.Send(new DeleteCategoryContentByStreetcodeIdQuery(streetcodeId, categoryId)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategoryContent([FromBody] CategoryContentCreateDto categoryContentCreateDto)
    {
        return HandleResult(await Mediator.Send(new CreateStreetcodeCategoryContentCommand(categoryContentCreateDto)));
    }
}
