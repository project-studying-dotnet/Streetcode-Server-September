using Streetcode.Identity.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Streetcode.Identity.Models.Dto;

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
    public UserRole Role { get; set; }
}
