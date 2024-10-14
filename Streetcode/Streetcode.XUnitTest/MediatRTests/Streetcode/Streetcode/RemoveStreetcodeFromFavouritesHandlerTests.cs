using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.RemoveFromFavourites;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.RemoveFromFavourites
{
    public class RemoveStreetcodeFromFavouritesHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly RemoveStreetcodeFromFavouritesHandler _handler;
        private const string CookieName = "favouritesStreetcodes";

        public RemoveStreetcodeFromFavouritesHandlerTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _handler = new RemoveStreetcodeFromFavouritesHandler(_httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_CookiePresentAndDoesNotContainStreetcode_DoesNotModifyCookie()
        {
            // Arrange
            int streetcodeId = 6; // Not in the list
            var initialFavourites = new List<int> { 1, 2, 3, 4, 5 };
            var favouritesString = string.Join(",", initialFavourites);

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Headers["Cookie"] = $"{CookieName}={favouritesString}";

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var command = new RemoveStreetcodeFromFavouritesCommand(streetcodeId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            Assert.False(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        }

        [Fact]
        public async Task Handle_CookieNotPresent_DoesNotModifyCookie()
        {
            // Arrange
            int streetcodeId = 3;

            var httpContext = new DefaultHttpContext();

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var command = new RemoveStreetcodeFromFavouritesCommand(streetcodeId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            Assert.False(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        }

        [Fact]
        public async Task Handle_HttpContextIsNull_ThrowsCustomException()
        {
            // Arrange
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

            var command = new RemoveStreetcodeFromFavouritesCommand(1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("No HttpContext in request", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        [Fact]
        public async Task Handle_CookieIsEmpty_DoesNotModifyCookie()
        {
            // Arrange
            int streetcodeId = 3;
            var favouritesString = ""; // Empty cookie

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Headers["Cookie"] = $"{CookieName}={favouritesString}";

            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var command = new RemoveStreetcodeFromFavouritesCommand(streetcodeId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            Assert.False(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        }
    }
}