using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetFavouritesList;

public record GetFavouritesListQuery : IRequest<Result<List<int>>>;