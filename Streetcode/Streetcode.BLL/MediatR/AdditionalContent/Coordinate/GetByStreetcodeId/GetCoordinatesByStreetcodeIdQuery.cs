using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.AdditionalContent.Coordinates.Types;

namespace Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId
{
    public record GetCoordinatesByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<StreetcodeCoordinateDto>>>;
}
