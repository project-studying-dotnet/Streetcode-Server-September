using Newtonsoft.Json;

namespace Streetcode.Identity.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string? Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
    public bool IsExpired => DateTime.Now >= ExpiryDate;

    public int? UserId { get; set; }

    [JsonIgnore]
    public ApplicationUser? User { get; set; }
}