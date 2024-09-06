using Streetcode.BLL.Dto.Partners;

namespace Streetcode.BLL.Dto.Team
{
    public class TeamMemberLinkDto
    {
        public int Id { get; set; }
        public LogoTypeDto LogoType { get; set; }
        public string TargetUrl { get; set; }
        public int TeamMemberId { get; set; }
    }
}
