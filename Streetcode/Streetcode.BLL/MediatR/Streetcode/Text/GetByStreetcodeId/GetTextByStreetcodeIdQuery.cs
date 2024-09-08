using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;

public record GetTextByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<TextDto?>>;