using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Streetcode.DAL.Enums;

namespace Streetcode.DAL.Entities.Users
{
    [Table("Users", Schema = "Users")]
    public class User: IdentityUser<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        [Required]
        [MaxLength(50)]
        public string Surname { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [MaxLength(20)]
        public string Login { get; set; } = null!;
        [Required]
        [MaxLength(20)]
        public string Password { get; set; } = null!;

        [Required]
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();

        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public UserRole Role { get; set; }
    }
}
