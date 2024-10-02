using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Security.Claims;
using System.Reflection;

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

        // Проверяем, аутентифицирован ли пользователь
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Проверяем, есть ли у пользователя требуемая роль
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == _role)
        {
            await next();
            return; // Авторизован, пользователь имеет нужную роль (например, Admin)
        }

        // Получаем ID пользователя из токена
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Пытаемся получить ID ресурса
        if (!TryGetResourceId(context, out string resourceId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        // Получаем UserId из комментария
        var UserIdInComment = await GetUserIdFromComment(context, resourceId);

        // Если пользователь является владельцем, разрешаем доступ
        if (userId == UserIdInComment)
        {
            await next();
            return; // Авторизован, пользователь является владельцем
        }

        // Если пользователь не админ и не владелец, запрещаем доступ
        context.Result = new ForbidResult();
    }

    private bool TryGetResourceId(ActionExecutingContext context, out string resourceId)
    {
        resourceId = null;

        // Сначала пытаемся получить ID из данных маршрута
        if (context.RouteData.Values.TryGetValue(_idParamName, out var idValue))
        {
            resourceId = idValue.ToString();
            return true;
        }

        // Если не удалось, пытаемся получить ID из аргументов действия
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null)
                continue;

            // Проверяем, есть ли свойство с именем _idParamName
            var property = arg.GetType().GetProperty(_idParamName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                var value = property.GetValue(arg);
                if (value != null)
                {
                    resourceId = value.ToString();
                    return true;
                }
            }

            // Альтернативно, ищем общие названия для ID
            var idPropertyNames = new[] { "Id", "CommentId", "ResourceId" };
            foreach (var propName in idPropertyNames)
            {
                property = arg.GetType().GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(arg);
                    if (value != null)
                    {
                        resourceId = value.ToString();
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private async Task<string> GetUserIdFromComment(ActionExecutingContext context, string id)
    {
        // Получаем RepositoryWrapper из сервисов
        var repositoryWrapper = context.HttpContext.RequestServices.GetService<IRepositoryWrapper>();

        if (repositoryWrapper == null)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return null;
        }

        // Ищем комментарий по ID и получаем UserId
        var comment = await repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(
            predicate: c => c.Id.ToString() == id
        );

        if (comment == null)
            throw new CustomException($"No comment found by entered Id - {id}", StatusCodes.Status204NoContent);

        return comment.UserId.ToString();
    }
}
