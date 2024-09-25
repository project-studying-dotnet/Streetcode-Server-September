using Microsoft.AspNetCore.Identity;

namespace Streedcode.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
