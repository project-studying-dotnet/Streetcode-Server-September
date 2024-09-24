using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.MediatR.Users.Login;
using Streetcode.BLL.MediatR.Users.Logout;
using Streetcode.BLL.MediatR.Users.Refresh;

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

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Logout(int userId)
        {
            var res = await Mediator.Send(new LogoutCommand(userId));
            return HandleResult(res);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Refresh(int userId)
        {
            var res = await Mediator.Send(new RefreshTokenCommand(userId));
            return HandleResult(res);
        }
    }
}
