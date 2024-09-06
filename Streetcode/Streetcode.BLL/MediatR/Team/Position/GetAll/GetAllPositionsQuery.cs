using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Team;

namespace Streetcode.BLL.MediatR.Team.Position.GetAll
{
    public record GetAllPositionsQuery : IRequest<Result<IEnumerable<PositionDto>>>;
}
