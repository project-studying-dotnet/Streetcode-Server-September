using Streetcode.Identity.Models.Dto;
namespace Streetcode.Identity.Services.Interfaces;

public interface IAuthService
{
   public Task<LoginResultDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
   public Task LogoutAsync(CancellationToken cancellationToken);
   public Task<string> RefreshJwtToken(CancellationToken cancellationToken);
}