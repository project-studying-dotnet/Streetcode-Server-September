using Microsoft.EntityFrameworkCore;
using Streetcode.Identity.Data;
using Streetcode.Identity.Models;

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
            .Where(rt => rt.UserId == userId)
            .ToListAsync();
    }

    public async Task<RefreshToken?> GetByIdAsync(int refreshTokenId)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Id == refreshTokenId);
    }

    public async Task<RefreshToken?> GetValidByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
         .FirstOrDefaultAsync(rt => rt.UserId == userId &&
                                     rt.ExpiryDate > DateTime.Now &&
                                     !rt.IsRevoked);
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

    public void Delete(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
  
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}