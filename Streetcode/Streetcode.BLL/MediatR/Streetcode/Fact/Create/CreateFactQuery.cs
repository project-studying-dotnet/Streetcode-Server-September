using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public record CreateFactQuery(FactDto Fact) : IRequest<Result<FactDto>>;
}
