using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder
{
    public record UpdateOrderFactCommand(FactOrderUpdateDto Fact) : IRequest<Result<IEnumerable<FactDto>>>;
}
