using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;

public record CreateRelatedTermCommand(RelatedTermCreateDto RelatedTerm) : IRequest<Result<RelatedTermFullDto>>;