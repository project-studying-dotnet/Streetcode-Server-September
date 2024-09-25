namespace Streedcode.TestWeb.Models
{
    public class PartnerDto
    {
        public int Id { get; set; }
        public bool IsKeyPartner { get; set; }
        public bool IsVisibleEverywhere { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int LogoId { get; set; }
    }
}
