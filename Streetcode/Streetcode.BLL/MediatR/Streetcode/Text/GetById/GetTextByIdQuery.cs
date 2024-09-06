using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetById;

public record GetTextByIdQuery(int Id) : IRequest<Result<TextDto>>;
