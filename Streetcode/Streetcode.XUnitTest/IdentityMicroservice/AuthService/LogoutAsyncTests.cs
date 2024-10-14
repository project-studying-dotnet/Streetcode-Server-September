using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Dto;
using Streetcode.Identity.Services.Interfaces;
using System.Security.Claims;
using Xunit;
using AuthServiceClass = Streetcode.Identity.Services.Realizations.AuthService;

namespace Streetcode.XUnitTest.IdentityMicroservice.AuthService;

public class LogoutAsyncTests
{
    private readonly AuthServiceClass _authService;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _claimsPrincipalFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public LogoutAsyncTests()
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
    public async Task LogoutAsync_ValidUser_RevokesRefreshToken()
    {
        // Arrange
        var userIdClaim = "1";
        _httpContextAccessorMock.Setup(h => h.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier))
                                .Returns(new Claim(ClaimTypes.NameIdentifier, userIdClaim));
        var refreshToken = new RefreshToken { UserId = 1, IsRevoked = false };

        _jwtServiceMock.Setup(js => js.GetValidRefreshTokenByUserIdAsync(1, CancellationToken.None)).ReturnsAsync(refreshToken);

        // Act
        await _authService.LogoutAsync(CancellationToken.None);

        // Assert
        Assert.True(refreshToken.IsRevoked);
        _signInManagerMock.Verify(sm => sm.SignOutAsync(), Times.Once);
    }


    [Fact]
    public async Task LogoutAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        // Arrange
        _httpContextAccessorMock.Setup(h => h.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier))
                                .Returns((Claim)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LogoutAsync(CancellationToken.None));
        Assert.Equal("Unauthorized", exception.Message);
    }

}
