using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Media.Art.Delete;

public record DeleteArtCommand(int id): IRequest<Result<Unit>>;
