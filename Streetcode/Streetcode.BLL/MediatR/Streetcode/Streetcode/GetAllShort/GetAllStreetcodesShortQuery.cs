using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAllShort
{
    public record GetAllStreetcodesShortQuery : IRequest<Result<IEnumerable<StreetcodeShortDto>>>;
}
