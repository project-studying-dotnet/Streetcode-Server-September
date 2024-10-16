using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.RemoveFromFavourites;

public record RemoveStreetcodeFromFavouritesCommand(int StreetcodeId) : IRequest<Result<Unit>>;