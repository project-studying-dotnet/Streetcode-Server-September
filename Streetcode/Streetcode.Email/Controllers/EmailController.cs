using Microsoft.AspNetCore.Mvc;
using Streetcode.Email.Models;
using Streetcode.Email.Resources;
using Streetcode.Email.Services;

namespace Streetcode.Email.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IEmailService _emailService;

        public EmailController(ILogger<EmailController> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMail([FromBody] SendMailDto sendMailDto)
        {
            var result = await _emailService.SendEmailAsync(sendMailDto);

            if(result)
                return Ok();
            else return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> SendRegistrationMail([FromBody] SendMailDto sendMailDto)
        {
            sendMailDto.Message = EmailTemplates.GetRegistrationEmailTemplate(sendMailDto.To, "https://streetcode.com.ua/");
            var result = await _emailService.SendEmailAsync(sendMailDto);

            if (result)
                return Ok();
            else return BadRequest();
        }
    }
}
