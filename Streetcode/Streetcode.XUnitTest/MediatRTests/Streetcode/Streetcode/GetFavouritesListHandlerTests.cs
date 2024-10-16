using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.GetFavouritesList;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Streetcode;

public class GetFavouritesListHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly GetFavouritesListHandler _handler;

    public GetFavouritesListHandlerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new GetFavouritesListHandler(_httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_CookiePresentAndValid_ReturnsFavouritesList()
    {
        // Arrange
        var cookieName = "favouritesStreetcodes";
        var favouritesString = "1,2,3,4,5";
        var expectedFavourites = new List<int> { 1, 2, 3, 4, 5 };
        var query = new GetFavouritesListQuery();

        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        var cookiesMock = new Mock<IRequestCookieCollection>();

        cookiesMock.Setup(c => c[cookieName]).Returns(favouritesString);
        requestMock.Setup(r => r.Cookies).Returns(cookiesMock.Object);
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFavourites, result.Value);
    }

    [Fact]
    public async Task Handle_CookieNotPresent_ReturnsEmptyList()
    {
        // Arrange
        var cookieName = "favouritesStreetcodes";
        var query = new GetFavouritesListQuery();

        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        var cookiesMock = new Mock<IRequestCookieCollection>();

        // Simulate absence of the cookie
        cookiesMock.Setup(c => c[cookieName]).Returns((string)null!);
        requestMock.Setup(r => r.Cookies).Returns(cookiesMock.Object);
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_CookieIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        var cookieName = "favouritesStreetcodes";
        var favouritesString = ""; // Empty cookie value
        var query = new GetFavouritesListQuery();

        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        var cookiesMock = new Mock<IRequestCookieCollection>();

        cookiesMock.Setup(c => c[cookieName]).Returns(favouritesString);
        requestMock.Setup(r => r.Cookies).Returns(cookiesMock.Object);
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_HttpContextIsNull_ThrowsCustomException()
    {
        // Arrange
        var query = new GetFavouritesListQuery();

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(query, CancellationToken.None));

        Assert.Equal("No HttpContext in request", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_CookieContainsInvalidData_ThrowsFormatException()
    {
        // Arrange
        var cookieName = "favouritesStreetcodes";
        var invalidFavouritesString = "1,2,abc,4,5"; // 'abc' is invalid
        var query = new GetFavouritesListQuery();

        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        var cookiesMock = new Mock<IRequestCookieCollection>();

        cookiesMock.Setup(c => c[cookieName]).Returns(invalidFavouritesString);
        requestMock.Setup(r => r.Cookies).Returns(cookiesMock.Object);
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _handler.Handle(query, CancellationToken.None));
    }
}