using Streedcode.TestWeb.Models;

namespace Streedcode.TestWeb.Services
{
    public interface IPartnerService
    {
        Task<IEnumerable<PartnerDto>> GetMovies();
    }
}
