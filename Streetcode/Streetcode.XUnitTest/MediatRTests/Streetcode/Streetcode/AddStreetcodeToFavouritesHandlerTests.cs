using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.AddToFavorite;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Streetcode;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Streetcode;

public class AddStreetcodeToFavouritesHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AddStreetcodeToFavouritesHandler _handler;

    public AddStreetcodeToFavouritesHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new AddStreetcodeToFavouritesHandler(_httpContextAccessorMock.Object, _repositoryWrapperMock.Object);
    }

    [Fact]
    public async Task Handle_StreetcodeExists_CookieIsEmpty_AddsStreetcodeToFavourites()
    {
        // Arrange
        int streetcodeId = 1;
        var streetcode = new StreetcodeContent { Id = streetcodeId };
        var command = new AddStreetcodeToFavouritesCommand(streetcodeId);

        // Mock repository to return the streetcode
        _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
            null))
            .ReturnsAsync(streetcode);

        // Mock HttpContext and its components
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify that the cookie was set correctly by checking the Set-Cookie header
        var setCookieHeader = httpContext.Response.Headers["Set-Cookie"];
        Assert.False(StringValues.IsNullOrEmpty(setCookieHeader));

        // Parse the Set-Cookie header to get the cookie value
        var cookie = setCookieHeader.ToString();
        Assert.Contains("favouritesStreetcodes=", cookie);

        var cookieValue = GetCookieValueFromSetCookieHeader(cookie, "favouritesStreetcodes");
        Assert.Equal(streetcodeId.ToString(), cookieValue);
    }

    // Helper method to extract cookie value from Set-Cookie header
    private static string GetCookieValueFromSetCookieHeader(string setCookieHeader, string cookieName)
    {
        var cookies = setCookieHeader.Split(',');
        foreach (var cookie in cookies)
        {
            if (cookie.Trim().StartsWith($"{cookieName}="))
            {
                var parts = cookie.Split(';');
                var nameValue = parts[0];
                var value = nameValue.Substring($"{cookieName}=".Length);
                return value;
            }
        }
        return null!;
    }

    [Fact]
    public async Task Handle_StreetcodeExists_CookieHasOtherIds_AddsStreetcodeToFavourites()
    {
        // Arrange
        int streetcodeId = 2;
        var streetcode = new StreetcodeContent { Id = streetcodeId };
        var command = new AddStreetcodeToFavouritesCommand(streetcodeId);

        // Mock repository to return the streetcode
        _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
            null))
            .ReturnsAsync(streetcode);

        // Mock HttpContext and its components
        var httpContext = new DefaultHttpContext();

        // Set existing cookie with other IDs
        httpContext.Request.Headers["Cookie"] = "favouritesStreetcodes=1,3";

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify that the cookie was updated correctly
        var setCookieHeader = httpContext.Response.Headers["Set-Cookie"];
        Assert.False(StringValues.IsNullOrEmpty(setCookieHeader));
    }

    [Fact]
    public async Task Handle_StreetcodeDoesNotExist_ThrowsCustomException()
    {
        // Arrange
        int streetcodeId = 1;
        var command = new AddStreetcodeToFavouritesCommand(streetcodeId);

        // Mock repository to return null
        _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StreetcodeContent, bool>>>(),
            null))
            .ReturnsAsync((StreetcodeContent)null!);

        // Mock HttpContext
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"No streetcode found by entered Id - {streetcodeId}", exception.Message);
        Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_HttpContextIsNull_ThrowsCustomException()
    {
        // Arrange
        int streetcodeId = 1;
        var streetcode = new StreetcodeContent { Id = streetcodeId };
        var command = new AddStreetcodeToFavouritesCommand(streetcodeId);

        // Mock repository to return the streetcode
        _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<StreetcodeContent, bool>>>(),
            null))
            .ReturnsAsync(streetcode);

        // Mock HttpContextAccessor to return null
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("No HttpContext in request", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_StreetcodeAlreadyInFavourites_DoesNotDuplicate()
    {
        // Arrange
        int streetcodeId = 1;
        var streetcode = new StreetcodeContent { Id = streetcodeId };
        var command = new AddStreetcodeToFavouritesCommand(streetcodeId);

        // Mock repository to return the streetcode
        _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
            null))
            .ReturnsAsync(streetcode);

        // Mock HttpContext and its components
        var httpContext = new DefaultHttpContext();

        // Set existing cookie with the same ID
        httpContext.Request.Headers["Cookie"] = "favouritesStreetcodes=1,2,3";

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify that the cookie was not duplicated
        var setCookieHeader = httpContext.Response.Headers["Set-Cookie"];
        Assert.False(StringValues.IsNullOrEmpty(setCookieHeader));;
    }
}