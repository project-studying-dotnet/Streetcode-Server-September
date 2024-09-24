using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Streetcode.DAL.Entities.Users
{
    [Table("Users", Schema = "Users")]
    public class User : IdentityUser<int>
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(50)]
        public string Surname { get; set; } = null!;
        [Required]
        public string? Role { get; set; }
        public int? RefreshTokenId { get; set; } 
        public RefreshToken? RefreshToken { get; set; } = null!;
    }
}
