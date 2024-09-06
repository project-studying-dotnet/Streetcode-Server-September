using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.Dto.Users
{
    public class UserLoginDto
    {
        [Required]
        [MaxLength(20)]
        public string Login { get; set; }
        [Required]
        [MaxLength(20)]
        public string Password { get; set; }
    }
}
