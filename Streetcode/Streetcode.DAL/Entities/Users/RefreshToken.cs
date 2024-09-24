namespace Streetcode.DAL.Entities.Users;
public class RefreshToken
{
    public int Id { get; set; }
    public string? Token { get; set; }
    public int? UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
    public bool IsExpired => DateTime.Now >= ExpiryDate;
    public User? User { get; set; }
}
