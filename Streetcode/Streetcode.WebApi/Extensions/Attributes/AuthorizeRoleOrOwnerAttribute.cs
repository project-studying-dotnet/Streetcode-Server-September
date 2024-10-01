using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Security.Claims;

namespace Streetcode.WebApi.Extensions.Attributes;

public class AuthorizeRoleOrOwnerAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;
    private readonly string _idParamName;

    public AuthorizeRoleOrOwnerAttribute(string role, string idParamName = "id")
    {
        _role = role;
        _idParamName = idParamName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        // Check if user is authenticated
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if the user has the specified role (e.g., Admin)
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == _role)
        {
            return;  // Authorized, user is in the role (Admin)
        }

        // Otherwise, check if the user is the owner (based on ID in the JWT token)
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // JWT usually has the user's ID in ClaimTypes.NameIdentifier
        if (userId == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Extract the ID from the route data (e.g., from /api/record/{id})
        var routeId = context.RouteData.Values[_idParamName]?.ToString();

        var UserIdInComment = getUserIdFromComment(context, routeId).Result;

        // If the user is the owner, allow access
        if (userId == UserIdInComment)
        {
            return;  // Authorized, user is the owner
        }

        // If not an admin or the owner, forbid access
        context.Result = new ForbidResult();
    }

    private async Task<string> getUserIdFromComment(AuthorizationFilterContext context, string id)
    {
        // Getting CommentRepository from services
        var repositoryWrapper = context.HttpContext.RequestServices.GetService<IRepositoryWrapper>();

        if (repositoryWrapper == null)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        // Search comment by ID and get UserId
        var comment = await repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(
            predicate: c => c.Id.ToString() == id
        );

        if (comment == null)
            throw new CustomException($"No comment found by entered Id - {id}", StatusCodes.Status204NoContent);

        return comment.UserId.ToString();
    }
}