namespace Streetcode.Identity.Models.Additional
{
    public class JwtVariables
    {
        public string Secret { get; set; } = null!;
        public int ExpirationInMinutes { get; set; }
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
    }
}
