using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Microsoft.Extensions.DependencyInjection;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.WebApi.Extensions.Attributes;

namespace Streetcode.XUnitTest.Extensions
{
    public class AuthorizeRoleOrOwnerAttributeTests
    {
        [Fact]
        public async Task OnActionExecutionAsync_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();

            var mockUser = new ClaimsPrincipal(new ClaimsIdentity());
            httpContext.User = mockUser;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                 actionContext,
                 new List<IFilterMetadata>(),
                 new Dictionary<string, object?>(),
                 controller: null!
             );

            var actionExecutedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                controller: null!
            );

            var next = new ActionExecutionDelegate(() => Task.FromResult(actionExecutedContext));

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, next);

            // Assert
            Assert.IsType<UnauthorizedResult>(actionExecutingContext.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_UserHasRequiredRole_Authorized()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var mockUser = new ClaimsPrincipal(identity);
            httpContext.User = mockUser;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller: null!
            );

            var actionExecutedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                controller: null!
            );

            var next = new ActionExecutionDelegate(() => Task.FromResult(actionExecutedContext));

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, next);

            // Assert
            Assert.Null(actionExecutingContext.Result); 
        }

        [Fact]
        public async Task OnActionExecutionAsync_UserIsOwner_Authorized()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var mockUser = new ClaimsPrincipal(identity);
            httpContext.User = mockUser;

            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            routeData.Values["id"] = "10";

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = routeData,
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller: null!
            );

            var actionExecutedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                controller: null!
            );

            var next = new ActionExecutionDelegate(() => Task.FromResult(actionExecutedContext));

            var mockRepoWrapper = new Mock<IRepositoryWrapper>();
            var mockCommentRepo = new Mock<ICommentRepository>();

            mockRepoWrapper.Setup(r => r.CommentRepository).Returns(mockCommentRepo.Object);
            mockCommentRepo.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                null))
                .ReturnsAsync(new Comment { Id = 10, UserId = 1 });

            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockRepoWrapper.Object)
                .BuildServiceProvider();
            httpContext.RequestServices = serviceProvider;

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, next);

            // Assert
            Assert.Null(actionExecutingContext.Result); 
        }

        [Fact]
        public async Task OnActionExecutionAsync_UserNotAdminOrOwner_ReturnsForbid()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "2") // UserId = 2
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var mockUser = new ClaimsPrincipal(identity);
            httpContext.User = mockUser;

            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            routeData.Values["id"] = "10";

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = routeData,
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                controller: null!
            );

            var actionExecutedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                controller: null!
            );

            var next = new ActionExecutionDelegate(() => Task.FromResult(actionExecutedContext));

            var mockRepoWrapper = new Mock<IRepositoryWrapper>();
            var mockCommentRepo = new Mock<ICommentRepository>();

            mockRepoWrapper.Setup(r => r.CommentRepository).Returns(mockCommentRepo.Object);
            mockCommentRepo.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                null))
                .ReturnsAsync(new Comment { Id = 10, UserId = 1 });

            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockRepoWrapper.Object)
                .BuildServiceProvider();
            httpContext.RequestServices = serviceProvider;

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, next);

            // Assert
            Assert.IsType<ForbidResult>(actionExecutingContext.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_UserIsOwner_WithIdInBody_Authorized()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var mockUser = new ClaimsPrincipal(identity);
            httpContext.User = mockUser;

            var commentDto = new CommentUpdateDto { Id = 10, CommentContent = "Updated content" };

            var actionArguments = new Dictionary<string, object>
            {
                { "commentDto", commentDto }
            };

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArguments!,
                controller: null!
            );

            var actionExecutedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                controller: null!
            );

            var next = new ActionExecutionDelegate(() => Task.FromResult(actionExecutedContext));

            var mockRepoWrapper = new Mock<IRepositoryWrapper>();
            var mockCommentRepo = new Mock<ICommentRepository>();

            mockRepoWrapper.Setup(r => r.CommentRepository).Returns(mockCommentRepo.Object);
            mockCommentRepo.Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Comment, bool>>>(),
                null))
                .ReturnsAsync(new Comment { Id = 10, UserId = 1 });

            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockRepoWrapper.Object)
                .BuildServiceProvider();
            httpContext.RequestServices = serviceProvider;

            // Act
            await attribute.OnActionExecutionAsync(actionExecutingContext, next);

            // Assert
            Assert.Null(actionExecutingContext.Result);
        }
    }
}
