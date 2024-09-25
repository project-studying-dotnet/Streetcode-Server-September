using System.ComponentModel.DataAnnotations;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.Dto.Users
{
    public class UserDto
    {
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
        public string UserName { get; set; } = null!;
        [Required]
        [MaxLength(20)]
        public string Password { get; set; } = null!;
        [Required]
        public UserRole Role { get; set; }
    }
}
