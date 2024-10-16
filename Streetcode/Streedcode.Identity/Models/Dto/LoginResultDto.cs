using Streetcode.Identity.Models.Dto;

namespace Streetcode.Identity.Models.Dto;

public class LoginResultDto
{
    public UserDataDto User { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
