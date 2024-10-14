using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.AddToFavorite;
public record AddStreetcodeToFavouritesCommand(int streetcodeId) : IRequest<Result<Unit>>;
