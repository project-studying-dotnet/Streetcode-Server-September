using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Newss.Delete
{
    public record DeleteFactCommand(int id) : IRequest<Result<Unit>>;
}
