using Microsoft.AspNetCore.Mvc;
using Streedcode.Identity.Models.Dto;

namespace Streedcode.Identity.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private ResponseDto _response;

        public AuthApiController(IRabbitMqSender rabbitMqSender)
        {
            _response = new();
        }
    }
}
