using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Create;

public record CreateTextCommand(TextCreateDto TextCreateDto): IRequest<Result<TextDto>>;