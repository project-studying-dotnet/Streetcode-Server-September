using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.Identity.Models.Dto;
using Streetcode.Identity.Services.Interfaces;

namespace Streetcode.Identity.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _loginService;

        public AuthApiController(IAuthService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _loginService.LoginAsync(loginDto);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh(int userId)
        {
            var result = await _loginService.RefreshJwtToken(userId);
            return Ok(result);
        }
    }
}
