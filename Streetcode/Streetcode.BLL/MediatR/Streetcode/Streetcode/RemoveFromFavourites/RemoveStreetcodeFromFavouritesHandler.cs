using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.RemoveFromFavourites;

public class RemoveStreetcodeFromFavouritesHandler : IRequestHandler<RemoveStreetcodeFromFavouritesCommand, Result<Unit>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CookieName = "favouritesStreetcodes";

    public RemoveStreetcodeFromFavouritesHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<Result<Unit>> Handle(RemoveStreetcodeFromFavouritesCommand request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext ?? throw new CustomException("No HttpContext in request", StatusCodes.Status400BadRequest);

        var favouritesCookie = context.Request.Cookies[CookieName];
        var favourites = new List<int>();

        if (!string.IsNullOrEmpty(favouritesCookie))
        {
            favourites = favouritesCookie.Split(',').Select(int.Parse).ToList();
        }

        if (favourites.Contains(request.StreetcodeId))
        {
            favourites.Remove(request.StreetcodeId);

            var newCookieValue = string.Join(",", favourites);
            context.Response.Cookies.Append(CookieName, newCookieValue, new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                Secure = true,
                HttpOnly = true
            });
        }

        return Task.FromResult(Result.Ok(Unit.Value));
    }
}