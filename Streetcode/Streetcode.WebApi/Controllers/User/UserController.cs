using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Users.Login;
using Streetcode.BLL.MediatR.Users.Logout;

namespace Streetcode.WebApi.Controllers.User
{
    public class UserController : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto) //dto later
        {
            var res = await Mediator.Send(new LoginCommand(userLoginDto));
            return HandleResult(res);
        }

        //[HttpGet("logout")]
        //public async Task<IActionResult> Logout() //dto later
        //{
        //    // 1. Remove refresh token
        //    // 2. _userManager logout
        //}

        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> RefreshJwtToken([FromBody] RefreshTokenRequest request)
        //{
            // 1. Check refreshToken existance 
            // 2. Check if user exists 
            // 3. Generate new JWT token 
        //}
    }
}
