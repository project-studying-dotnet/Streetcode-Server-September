using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.MediatR.Users.Login;

namespace Streetcode.WebApi.Controllers.User
{
    public class UserController : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var res = await Mediator.Send(new LoginCommand(userLoginDto));
            return HandleResult(res);
        }
    }
}
