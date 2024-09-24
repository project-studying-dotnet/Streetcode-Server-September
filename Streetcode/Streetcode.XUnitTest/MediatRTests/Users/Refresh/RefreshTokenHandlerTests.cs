using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.MediatR.Users.Refresh;
using Streetcode.DAL.Entities.Users;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Users.Refresh;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly RefreshTokenHandler _handler;

    public RefreshTokenHandlerTests()
    {
        _jwtServiceMock = new Mock<IJwtService>();

        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null, null, null, null, null, null, null, null);

        _handler = new RefreshTokenHandler(_jwtServiceMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_RefreshTokenNotFound_ThrowsCustomException()
    {
        // Arrange
        int userId = 1;
        var command = new RefreshTokenCommand(userId);

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenByUserIdAsync(command.userId))
            .ReturnsAsync((RefreshToken)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Invalid or expired refresh token. Log in again", exception.Message);
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_RefreshTokenRevoked_ThrowsCustomException()
    {
        // Arrange
        int userId = 1;
        var command = new RefreshTokenCommand(userId);
        var revokedToken = new RefreshToken { IsRevoked = true };

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenByUserIdAsync(command.userId))
            .ReturnsAsync(revokedToken);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Invalid or expired refresh token. Log in again", exception.Message);
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_RefreshTokenExpired_ThrowsCustomException()
    {
        // Arrange
        int userId = 1;
        var command = new RefreshTokenCommand(userId);
        var expiredToken = new RefreshToken { ExpiryDate = DateTime.Now.AddDays(-1) };

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenByUserIdAsync(command.userId))
            .ReturnsAsync(expiredToken);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Invalid or expired refresh token. Log in again", exception.Message);
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewJwtToken()
    {
        // Arrange
        int userId = 1;
        var command = new RefreshTokenCommand(userId);
        var validToken = new RefreshToken { ExpiryDate = DateTime.Now.AddDays(1), IsRevoked = false, UserId = command.userId };
        var user = new User { Id = command.userId };
        var newJwtToken = "new-jwt-token";

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenByUserIdAsync(command.userId))
            .ReturnsAsync(validToken);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(command.userId.ToString()))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(x => x.CreateJwtTokenAsync(user))
            .ReturnsAsync(newJwtToken); // Simulate JWT token creation

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newJwtToken, result.Value);

        _jwtServiceMock.Verify(x => x.GetRefreshTokenByUserIdAsync(command.userId), Times.Once);
        _userManagerMock.Verify(x => x.FindByIdAsync(command.userId.ToString()), Times.Once);
        _jwtServiceMock.Verify(x => x.CreateJwtTokenAsync(user), Times.Once);
    }
}