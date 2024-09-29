using Microsoft.AspNetCore.Identity;

namespace Streetcode.Identity.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Role { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
