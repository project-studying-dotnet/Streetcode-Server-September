using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;

namespace Streetcode.BLL.MediatR.Streetcode.Term.GetById;

public record GetTermByIdQuery(int Id): IRequest<Result<TermDto>>;
