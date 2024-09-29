using Streetcode.Identity.Models.Dto;
namespace Streetcode.Identity.Services.Interfaces;

public interface IAuthService
{
   public Task<LoginResultDto> LoginAsync(LoginDto loginDto);
   public Task<string> RefreshJwtToken(int userId);
}