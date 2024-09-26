using Microsoft.AspNetCore.Identity;

namespace Streedcode.Identity.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Role { get; set; } = null!;
}
