using Streetcode.Identity.Models;
using System.Threading.Tasks;

namespace Streetcode.Identity.Services.Interfaces;

public interface IJwtService
{
   public Task<string> CreateJwtTokenAsync(ApplicationUser user);
   public Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user);
   public Task<List<RefreshToken>> GetAllRefreshsTokenByUserIdAsync(int id, CancellationToken cancellationToken);
   public Task<RefreshToken> GetValidRefreshTokenByUserIdAsync(int id, CancellationToken cancellationToken);
   public Task DeleteInvalidTokensAsync();
   public Task UpdateTokenAsync(RefreshToken token);
}
