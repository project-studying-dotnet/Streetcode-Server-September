﻿using Streetcode.Identity.Models;

namespace Streetcode.Identity.Services.Interfaces;

public interface IJwtService
{
   public Task<string> CreateJwtTokenAsync(ApplicationUser user);
   public Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user);
   public Task<List<RefreshToken>> GetAllRefreshsTokenByUserIdAsync(int id, CancellationToken cancellationToken);
   public Task<RefreshToken> GetValidRefreshTokenByUserIdAsync(int id, CancellationToken cancellationToken);
}
