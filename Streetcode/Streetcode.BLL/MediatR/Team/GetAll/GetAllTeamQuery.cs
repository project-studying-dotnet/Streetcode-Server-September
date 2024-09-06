using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Team;

namespace Streetcode.BLL.MediatR.Team.GetAll
{
    public record GetAllTeamQuery : IRequest<Result<IEnumerable<TeamMemberDto>>>;
}
