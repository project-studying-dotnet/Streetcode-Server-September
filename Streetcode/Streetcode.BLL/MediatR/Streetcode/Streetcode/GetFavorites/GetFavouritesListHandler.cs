using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetFavouritesList;

public class GetFavouritesListHandler : IRequestHandler<GetFavouritesListQuery, Result<List<int>>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CookieName = "favouritesStreetcodes";

    public GetFavouritesListHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Result<List<int>>> Handle(GetFavouritesListQuery request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext ?? throw new CustomException("No HttpContext in request", StatusCodes.Status400BadRequest);

        var favouritesCookie = context.Request.Cookies[CookieName];
        var favourites = new List<int>();

        if (!string.IsNullOrEmpty(favouritesCookie))
        {
            favourites = favouritesCookie.Split(',').Select(int.Parse).ToList();
        }

        return Task.FromResult(Result.Ok(favourites));
    }
}
