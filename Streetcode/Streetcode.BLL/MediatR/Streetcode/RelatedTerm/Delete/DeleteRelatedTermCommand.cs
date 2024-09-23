using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;

public record DeleteRelatedTermCommand(string Word, int TermId): IRequest<Result<Unit>>;