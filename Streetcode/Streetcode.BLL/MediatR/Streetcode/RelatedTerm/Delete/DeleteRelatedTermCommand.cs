using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete
{
    public record DeleteRelatedTermCommand(string word): IRequest<Result<RelatedTermDto>>;
}
