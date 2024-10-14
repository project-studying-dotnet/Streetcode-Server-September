using System;
using System.Collections.Generic;
using System.Linq;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.AddToFavorite;

public class AddStreetcodeToFavouritesHandler : IRequestHandler<AddStreetcodeToFavouritesCommand, Result<Unit>>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public AddStreetcodeToFavouritesHandler(IHttpContextAccessor httpContextAccessor, IRepositoryWrapper repositoryWrapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<Unit>> Handle(AddStreetcodeToFavouritesCommand request, CancellationToken cancellationToken)
    {
        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(s => s.Id == request.streetcodeId);

        if (streetcode == null)
            throw new CustomException($"No streetcode found by entered Id - {request.streetcodeId}", StatusCodes.Status204NoContent);

        const string CookieName = "favouritesStreetcodes";

        var context = _httpContextAccessor.HttpContext ?? throw new CustomException("No HttpContext in request", StatusCodes.Status400BadRequest);

        var favouritesCookie = context.Request.Cookies[CookieName];
        var favourites = new List<int>();

        if (!string.IsNullOrEmpty(favouritesCookie))
        {
            // Parse the existing favourites from the cookie
            favourites = favouritesCookie.Split(',').Select(int.Parse).ToList();
        }

        // Add the new Streetcode to the favourites list
        if (!favourites.Contains(request.streetcodeId))
        {
            favourites.Add(request.streetcodeId);
        }

        // Update the favourites cookie (storing as a comma-separated list)
        var newCookieValue = string.Join(",", favourites);
        context.Response.Cookies.Append(CookieName, newCookieValue, new CookieOptions
        {
            Expires = DateTime.Now.AddYears(1),
            Secure = true,
            HttpOnly = true
        });

        return Result.Ok(Unit.Value);
    }
}
