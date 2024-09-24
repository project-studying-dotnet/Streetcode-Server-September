using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Refresh;
using Streetcode.DAL.Repositories.Interfaces.Users;
using Streetcode.DAL.Repositories.Realizations.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.DAL.Repositories.Realizations.Users
{
    public class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(StreetcodeDbContext context)
            : base(context)
        {
        }
    }
}
