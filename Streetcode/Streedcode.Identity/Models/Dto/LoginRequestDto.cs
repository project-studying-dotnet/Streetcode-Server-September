using System.ComponentModel.DataAnnotations;

namespace Streedcode.Identity.Models.Dto
{
    public class LoginRequestDto
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
