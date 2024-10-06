using Microsoft.EntityFrameworkCore;
using Streetcode.Identity.Data;
using Streetcode.Identity.Models;
using System.Linq.Expressions;

namespace Streetcode.Identity.Repository;

public class RefreshRepository : IRefreshRepository
{
    private readonly AppDbContext _context;

    public RefreshRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RefreshToken>> GetByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId).ToListAsync();
    }

    public async Task<RefreshToken?> GetByIdAsync(int refreshTokenId)
    {
        return await _context.RefreshTokens
            .OrderByDescending(rt => rt.ExpiryDate)
            .FirstOrDefaultAsync(rt => rt.Id == refreshTokenId);
    }

    public async Task<RefreshToken?> GetValidByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
         .FirstOrDefaultAsync(rt => rt.UserId == userId &&
                                     rt.ExpiryDate > DateTime.Now &&
                                     !rt.IsRevoked);
    }

    public async Task<IEnumerable<RefreshToken>> GetAllAsync(Expression<Func<RefreshToken, bool>> predicate = null)
    {
        if (predicate != null)
        {
            return await _context.RefreshTokens.Where(predicate).ToListAsync();
        }

        return await _context.RefreshTokens.ToListAsync();
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        return refreshToken;
    }

    public void Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }
    public void Delete(IEnumerable<RefreshToken> refreshTokens)
    {
        _context.RefreshTokens.RemoveRange(refreshTokens);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}