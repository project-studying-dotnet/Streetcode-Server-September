using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Analytics;
using Streetcode.BLL.MediatR.Analytics.Create;
using Streetcode.BLL.MediatR.Analytics.Delete;

namespace Streetcode.WebApi.Controllers.Analytics
{
    public class StatisticRecordController: BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StatisticRecordCreateDto newRecord)
        {
            return HandleResult(await Mediator.Send(new CreateStatisticRecordCommand(newRecord)));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            return HandleResult(await Mediator.Send(new DeleteStatisticRecordCommand(id)));
        }
    }
}
