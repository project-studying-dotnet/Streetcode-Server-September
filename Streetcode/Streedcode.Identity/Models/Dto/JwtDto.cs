namespace Streetcode.Identity.Models.Dto;

public class JwtDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
}
