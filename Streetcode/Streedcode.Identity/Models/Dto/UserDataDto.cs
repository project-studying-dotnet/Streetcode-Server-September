using System.ComponentModel.DataAnnotations;

namespace Streedcode.Identity.Models.Dto;
public class UserDataDto
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Email { get; set; } = null!;
}