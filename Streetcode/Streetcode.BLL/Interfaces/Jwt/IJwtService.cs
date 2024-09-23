using Streetcode.DAL.Entities.Users;

namespace Streetcode.BLL.Interfaces.Jwt
{
    public interface IJwtService
    {
        public Task<string> Create(User user);
        //public string GenerateRefreshTokenString();
    }
}
