using Streetcode.Identity.Models;

namespace Streetcode.Identity.Repository;

public interface IRefreshRepository
{
    Task<List<RefreshToken>> GetByUserIdAsync(int userId);
    Task<RefreshToken?> GetByIdAsync(int refreshTokenId);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetValidByUserIdAsync(int userId);
    void Update(RefreshToken refreshToken);
    void Delete(RefreshToken refreshToken);
    Task<int> SaveChangesAsync();
}
