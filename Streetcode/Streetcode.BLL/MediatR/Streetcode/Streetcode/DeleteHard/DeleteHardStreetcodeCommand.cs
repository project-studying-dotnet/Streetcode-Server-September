using FluentResults;
using MediatR;


namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.DeleteHard
{
    public record DeleteHardStreetcodeCommand(int Id): IRequest<Result<Unit>>;
}
