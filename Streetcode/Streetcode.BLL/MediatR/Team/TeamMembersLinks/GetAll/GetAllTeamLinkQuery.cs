using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Team;

namespace Streetcode.BLL.MediatR.Team.TeamMembersLinks.GetAll
{
    public record GetAllTeamLinkQuery : IRequest<Result<IEnumerable<TeamMemberLinkDto>>>;
}
