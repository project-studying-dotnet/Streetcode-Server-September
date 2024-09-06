using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent;

namespace Streetcode.BLL.MediatR.Streetcode.Term.GetAll
{
    public record GetAllTermsQuery : IRequest<Result<IEnumerable<TermDto>>>;
}
