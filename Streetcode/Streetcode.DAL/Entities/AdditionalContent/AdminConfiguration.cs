using Streetcode.DAL.Enums;

namespace Streetcode.DAL.Entities.AdditionalContent;

public class AdminConfiguration
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRole Role { get; set; }
}