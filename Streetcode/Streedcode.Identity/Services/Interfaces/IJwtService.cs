using Streetcode.Identity.Models;

namespace Streetcode.Identity.Services.Interfaces;

public interface IJwtService
{
   public Task<string> Create(ApplicationUser user);
}
