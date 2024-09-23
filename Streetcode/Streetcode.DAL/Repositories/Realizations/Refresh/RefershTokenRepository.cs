using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Refresh;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.Refresh
{
    public class RefershTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
    {
        public RefershTokenRepository(StreetcodeDbContext context) : base(context)
        {
        }
    }
}
