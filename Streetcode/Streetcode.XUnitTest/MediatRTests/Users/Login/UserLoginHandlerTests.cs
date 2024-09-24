using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.BLL.Dto.Users;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.BLL.MediatR.Users.Login;
using Streetcode.DAL.Entities.Users;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Users.Login;

public class UserLoginHandlerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserLoginHandler _handler;

    public UserLoginHandlerTests()
    {
        _userManagerMock = MockUserManager();
        _signInManagerMock = MockSignInManager();
        _jwtServiceMock = new Mock<IJwtService>();
        _mapperMock = new Mock<IMapper>();

        _handler = new UserLoginHandler(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsCustomException()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@test.com", Password = "password" };
        var loginCommand = new LoginCommand(loginDto);

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() =>
            _handler.Handle(loginCommand, CancellationToken.None)
        );

        Assert.Equal("User not found", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsCustomException()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@test.com", Password = "wrongpassword" };
        var loginCommand = new LoginCommand(loginDto);

        var user = new User { Email = loginDto.Email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user); 

        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() =>
            _handler.Handle(loginCommand, CancellationToken.None)
        );

        Assert.Equal("Incorrect log in data", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_Success_ReturnsLoginResultDto()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@test.com", Password = "password" };
        var loginCommand = new LoginCommand(loginDto);

        var user = new User { Email = loginDto.Email };
        var jwtToken = "test-jwt-token";
        var refreshToken = new RefreshToken
        {
            Token = "test-refresh-token",
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(1) 
        };
        var userDataDto = new UserDataDto { Email = user.Email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user); 

        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false, true))
            .ReturnsAsync(SignInResult.Success); 

        _jwtServiceMock
            .Setup(x => x.CreateJwtTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(jwtToken);

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(refreshToken);

        _mapperMock
            .Setup(x => x.Map<UserDataDto>(It.IsAny<User>()))
            .Returns(userDataDto); 

        // Act
        var result = await _handler.Handle(loginCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(jwtToken, result.Value.JwtToken);
        Assert.Equal(refreshToken.Token, result.Value.RefreshToken);
        Assert.Equal(userDataDto, result.Value.User);
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
