using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Dto.Streetcode.TextContent;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedFigure.Create;

public record CreateRelatedFigureCommand(int ObserverId, int TargetId) : IRequest<Result<Unit>>;
