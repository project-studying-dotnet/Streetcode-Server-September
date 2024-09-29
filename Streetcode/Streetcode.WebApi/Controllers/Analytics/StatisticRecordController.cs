using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Analytics;
using Streetcode.BLL.MediatR.Analytics.Create;

namespace Streetcode.WebApi.Controllers.Analytics
{
    public class StatisticRecordController: BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StatisticRecordCreateDto newRecord)
        {
            return HandleResult(await Mediator.Send(new CreateStatisticRecordCommand(newRecord)));
        }
    }
}
