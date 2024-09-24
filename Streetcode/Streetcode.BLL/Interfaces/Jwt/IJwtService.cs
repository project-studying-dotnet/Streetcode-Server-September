using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.Interfaces.Jwt
{
    public interface IJwtService
    {
        public Task<string> CreateJwtTokenAsync(User user);
        public Task<RefreshToken> CreateRefreshTokenAsync(User user);
        public Task<RefreshToken> GetRefreshTokenByUserIdAsync(int id);
        public Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        public Task DeleteRefreshTokenAsync(int id);
    }
}
