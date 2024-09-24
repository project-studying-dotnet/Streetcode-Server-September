using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.MediatR.Users.Login;
using Streetcode.BLL.MediatR.Users.Logout;
using Streetcode.DAL.Entities.Users;
using System;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Users.Logout;

public class UserLogoutHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly UserLogoutHandler _handler;

    public UserLogoutHandlerTests()
    {
        _userManagerMock = MockUserManager();
        _signInManagerMock = MockSignInManager();
        _jwtServiceMock = new Mock<IJwtService>();

        _handler = new UserLogoutHandler(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsCustomException()
    {
        // Arrange
        int userId = 1;
        var logoutCommand = new LogoutCommand(userId);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<int>().ToString()))
            .ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() =>
            _handler.Handle(logoutCommand, CancellationToken.None)
        );

        Assert.Equal("User not found", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_RevokesRefreshTokenAndSignsOut_ReturnsOk()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId };

        var refreshToken = new RefreshToken
        {
            Token = "test-token",
            UserId = userId,
            IsRevoked = false
        };

        var command = new LogoutCommand(userId);

        _userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user); 

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenByUserIdAsync(userId))
            .ReturnsAsync(refreshToken); 

        _jwtServiceMock
            .Setup(x => x.UpdateRefreshTokenAsync(refreshToken))
            .Returns(Task.CompletedTask);

        _signInManagerMock
            .Setup(x => x.SignOutAsync())
            .Returns(Task.CompletedTask); 

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Unit.Value, result.Value);
        Assert.True(refreshToken.IsRevoked);

        _jwtServiceMock.Verify(x => x.GetRefreshTokenByUserIdAsync(userId), Times.Once);
        _jwtServiceMock.Verify(x => x.UpdateRefreshTokenAsync(refreshToken), Times.Once);
        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
    }

    private static Mock<UserManager<User>> MockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<SignInManager<User>> MockSignInManager()
    {
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
        var userManagerMock = MockUserManager();

        return new Mock<SignInManager<User>>(
            userManagerMock.Object,
            contextAccessorMock.Object,
            userPrincipalFactoryMock.Object,
            null, null, null, null
        );
    }
}
