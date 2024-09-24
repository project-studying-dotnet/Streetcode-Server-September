using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.Dto.Users
{
    public class UserLoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
