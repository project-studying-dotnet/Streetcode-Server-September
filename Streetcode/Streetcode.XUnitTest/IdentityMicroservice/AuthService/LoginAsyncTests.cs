using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Dto;
using Streetcode.Identity.Services.Interfaces;
using System.Security.Claims;
using Xunit;

using AuthServiceClass = Streetcode.Identity.Services.Realizations.AuthService;

namespace Streetcode.XUnitTest.IdentityMicroservice.AuthService;


public class AuthServiceTests
{
    private readonly AuthServiceClass _authService;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _claimsPrincipalFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    public AuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, 
            null, null, null, null, null, null, null, null
        );

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _claimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object, 
            _httpContextAccessorMock.Object, 
            _claimsPrincipalFactoryMock.Object, 
            null, null, null, null
        );

        _jwtServiceMock = new Mock<IJwtService>();
        _mapperMock = new Mock<IMapper>();
        _configurationMock = new Mock<IConfiguration>();

        _authService = new AuthServiceClass(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _mapperMock.Object,
            _httpContextAccessorMock.Object,
            _configurationMock.Object,
            null
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResultDto()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };
        var user = new ApplicationUser { Email = "test@example.com", Id = 1 };
        var jwtToken = "sample_jwt_token";
        var refreshToken = new RefreshToken { Token = "sample_refresh_token", UserId = user.Id };

        _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(sm => sm.PasswordSignInAsync(user, loginDto.Password, false, true))
                          .ReturnsAsync(SignInResult.Success);
        _jwtServiceMock.Setup(js => js.CreateJwtTokenAsync(user)).ReturnsAsync(jwtToken);
        _jwtServiceMock.Setup(js => js.CreateRefreshTokenAsync(user)).ReturnsAsync(refreshToken);
        _mapperMock.Setup(m => m.Map<UserDataDto>(user)).Returns(new UserDataDto());

        // Act
        var result = await _authService.LoginAsync(loginDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jwtToken, result.Token);
        Assert.Equal(refreshToken.Token, result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "unknown@example.com", Password = "password123" };
        
        _userManagerMock.Setup(um => um!.FindByEmailAsync(loginDto.Email))!.ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _authService.LoginAsync(loginDto, CancellationToken.None));
        Assert.Equal("User not found", exception.Message);
    }
}
