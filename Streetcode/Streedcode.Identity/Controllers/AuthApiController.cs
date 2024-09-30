using Microsoft.AspNetCore.Mvc;
using Streedcode.Identity.MessageBroker;
using Streedcode.Identity.Models.Dto;

namespace Streedcode.Identity.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private ResponseDto _response;
        private readonly IRabbitMqSender _messageBus;

        public AuthApiController(IRabbitMqSender rabbitMqSender)
        {
            _response = new();
            _messageBus = rabbitMqSender;
        }
    }
}
