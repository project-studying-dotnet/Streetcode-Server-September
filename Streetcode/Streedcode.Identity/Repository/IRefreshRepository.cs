using Streetcode.Identity.Models;
using System.Linq.Expressions;

namespace Streetcode.Identity.Repository;

public interface IRefreshRepository
{
    Task<List<RefreshToken>> GetByUserIdAsync(int userId);
    Task<RefreshToken?> GetByIdAsync(int refreshTokenId);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetValidByUserIdAsync(int userId);
    Task<IEnumerable<RefreshToken>> GetAllAsync(Expression<Func<RefreshToken, bool>> predicate);
    void Update(RefreshToken refreshToken);
    void Delete(IEnumerable<RefreshToken> refreshTokens);
    Task<int> SaveChangesAsync();
}
