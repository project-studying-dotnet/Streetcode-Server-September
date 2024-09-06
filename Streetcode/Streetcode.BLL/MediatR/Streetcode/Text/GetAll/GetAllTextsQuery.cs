using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetAll;

public record GetAllTextsQuery : IRequest<Result<IEnumerable<TextDto>>>;