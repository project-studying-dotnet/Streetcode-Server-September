using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Dto.Team;

namespace Streetcode.BLL.MediatR.Team.GetById
{
    public record GetByIdTeamQuery(int Id) : IRequest<Result<TeamMemberDto>>;
}
