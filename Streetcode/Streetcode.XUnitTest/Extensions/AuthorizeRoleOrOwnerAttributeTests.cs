using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Microsoft.Extensions.DependencyInjection;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.WebApi.Extensions.Attributes;

namespace Streetcode.XUnitTest.Extensions
{
    public class AuthorizeRoleOrOwnerAttributeTests
    {
        [Fact]
        public void OnAuthorization_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();
            var authFilterContext = CreateAuthorizationFilterContext(httpContext);

            var mockUser = new ClaimsPrincipal(new ClaimsIdentity());
            httpContext.User = mockUser;

            // Act
            attribute.OnAuthorization(authFilterContext);

            // Assert
            Assert.IsType<UnauthorizedResult>(authFilterContext.Result);
        }

        [Fact]
        public void OnAuthorization_UserHasRequiredRole_Authorized()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();
            var authFilterContext = CreateAuthorizationFilterContext(httpContext);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var mockUser = new ClaimsPrincipal(identity);
            httpContext.User = mockUser;

            // Act
            attribute.OnAuthorization(authFilterContext);

            // Assert
            Assert.Null(authFilterContext.Result);
        }

        [Fact]
        public async Task OnAuthorizationAsync_UserIsOwner_Authorized()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();
            var authFilterContext = CreateAuthorizationFilterContext(httpContext);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var mockUser = new ClaimsPrincipal(identity);
            httpContext.User = mockUser;

            authFilterContext.RouteData.Values["id"] = "10";

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
            attribute.OnAuthorization(authFilterContext);

            // Assert
            Assert.Null(authFilterContext.Result);
        }


        [Fact]
        public async Task OnAuthorizationAsync_UserNotAdminOrOwner_ReturnsForbid()
        {
            // Arrange
            var attribute = new AuthorizeRoleOrOwnerAttribute("Admin");
            var httpContext = new DefaultHttpContext();
            var authFilterContext = CreateAuthorizationFilterContext(httpContext);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "2")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var mockUser = new ClaimsPrincipal(identity);
            httpContext.User = mockUser;

            authFilterContext.RouteData.Values["id"] = "10";

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
            attribute.OnAuthorization(authFilterContext);

            // Assert
            Assert.IsType<ForbidResult>(authFilterContext.Result);
        }


        private AuthorizationFilterContext CreateAuthorizationFilterContext(HttpContext httpContext)
        {
            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
        }
    }
}