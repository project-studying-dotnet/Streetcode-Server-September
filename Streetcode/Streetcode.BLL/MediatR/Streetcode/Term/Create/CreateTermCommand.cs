using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Create;

public record CreateTermCommand(TermCreateDto TermCreateDto): IRequest<Result<int>>;