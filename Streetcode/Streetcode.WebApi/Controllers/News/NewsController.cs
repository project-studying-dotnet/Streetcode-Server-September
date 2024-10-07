using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.MediatR.Newss.SortedByDateTime;

namespace Streetcode.WebApi.Controllers.News;

public class NewsController: BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> SortedNewsByDateTime()
    {
        return HandleResult(await Mediator.Send(new SortedByDateTimeQuery()));
    }
}
