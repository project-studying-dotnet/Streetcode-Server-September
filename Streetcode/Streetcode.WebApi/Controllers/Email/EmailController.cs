using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Email;
using Streetcode.BLL.MediatR.Email;

namespace Streetcode.WebApi.Controllers.Email
{
  public class EmailController : BaseApiController
  {
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] EmailDto email)
    {
      return HandleResult(await Mediator.Send(new SendEmailCommand(email)));
    }
  }
}
