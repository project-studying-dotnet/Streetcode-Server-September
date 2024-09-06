using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetShortById
{
    public record GetStreetcodeShortByIdQuery(int id) : IRequest<Result<StreetcodeShortDto>>
    {
    }
}
