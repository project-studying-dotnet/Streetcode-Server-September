using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Streedcode.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(50)]
        public string Surname { get; set; } = null!;
    }
}
