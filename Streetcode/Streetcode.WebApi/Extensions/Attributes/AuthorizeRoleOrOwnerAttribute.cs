using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Security.Claims;
using System.Reflection;

namespace Streetcode.WebApi.Extensions.Attributes;

public class AuthorizeRoleOrOwnerAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _role;
    private readonly string _idParamName;

    public AuthorizeRoleOrOwnerAttribute(string role, string idParamName = "id")
    {
        _role = role;
        _idParamName = idParamName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        // Check if the user is authenticated
        if (!user.Identity!.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Check if the user has the required role
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == _role)
        {
            await next();
            return; // Authorized, the user has the required role (e.g. Admin)
        }

        // Get user ID from token
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        //  Trying to get the resource ID
        if (!TryGetResourceId(context, out string resourceId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        // Getting UserId from comment
        var UserIdInComment = await GetUserIdFromComment(context, resourceId);

        // If the user is the owner, allow access
        if (userId == UserIdInComment)
        {
            await next();
            return; // Authorized, the user is the owner
        }

        // If the user is neither admin nor owner, deny access
        context.Result = new ForbidResult();
    }

    private bool TryGetResourceId(ActionExecutingContext context, out string? resourceId)
    {
        resourceId = null;

        // First we try to get the ID from the route data
        if (context.RouteData.Values.TryGetValue(_idParamName, out var idValue))
        {
            resourceId = idValue!.ToString();
            return true;
        }

        // If unsuccessful, try to get the ID from the action arguments
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null)
                continue;

            // Check if there is a property named _idParamName
            var property = arg.GetType().GetProperty(_idParamName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                var value = property.GetValue(arg);
                if (value != null)
                {
                    resourceId = value!.ToString();
                    return true;
                }
            }

            // Alternatively, look for common names for IDs
            var idPropertyNames = new[] { "Id", "CommentId", "ResourceId" };
            foreach (var propName in idPropertyNames)
            {
                property = arg.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(arg);
                    if (value != null)
                    {
                        resourceId = value!.ToString();
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static async Task<string?> GetUserIdFromComment(ActionExecutingContext context, string id)
    {
        // Getting RepositoryWrapper from services
        var repositoryWrapper = context.HttpContext.RequestServices.GetService<IRepositoryWrapper>();

        if (repositoryWrapper == null)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return null;
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
