using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Streetcode.Identity.Models;
using Streetcode.Identity.Services.Interfaces;
using System.Security.Claims;
using Xunit;
using AuthServiceClass = Streetcode.Identity.Services.Realizations.AuthService;

namespace Streetcode.XUnitTest.IdentityMicroservice.AuthService;

public class RefreshJwtTokenTests
{
    private readonly AuthServiceClass _authService;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IUserClaimsPrincipalFactory<ApplicationUser>> _claimsPrincipalFactoryMock;

    public RefreshJwtTokenTests()
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

        _authService = new AuthServiceClass(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _jwtServiceMock.Object,
            _mapperMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    [Fact]
    public async Task RefreshJwtToken_ValidRefreshToken_ReturnsNewJwtToken()
    {
        // Arrange
        var userIdClaim = "1";
        _httpContextAccessorMock.Setup(h => h.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier))
                                .Returns(new Claim(ClaimTypes.NameIdentifier, userIdClaim));
        var refreshToken = new RefreshToken { UserId = 1 };

        _jwtServiceMock.Setup(js => js.GetValidRefreshTokenByUserIdAsync(1, CancellationToken.None)).ReturnsAsync(refreshToken);
        var user = new ApplicationUser { Id = 1 };
        _userManagerMock.Setup(um => um.FindByIdAsync(refreshToken.UserId.ToString())).ReturnsAsync(user);
        var newJwtToken = "new_jwt_token";
        _jwtServiceMock.Setup(js => js.CreateJwtTokenAsync(user)).ReturnsAsync(newJwtToken);

        // Act
        var result = await _authService.RefreshJwtToken(CancellationToken.None);

        // Assert
        Assert.Equal(newJwtToken, result);
    }

    [Fact]
    public async Task RefreshJwtToken_UserNotAuthenticated_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _httpContextAccessorMock.Setup(h => h.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier))
                                .Returns((Claim)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                                                () => _authService.RefreshJwtToken(CancellationToken.None));
        Assert.Equal("Unauthorized", exception.Message);
    }

    [Fact]
    public async Task RefreshJwtToken_NoValidRefreshToken_ThrowsKeyNotFoundException()
    {
        // Arrange
        var userIdClaim = "1";
        _httpContextAccessorMock.Setup(h => h.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
                                .Returns(new Claim(ClaimTypes.NameIdentifier, userIdClaim));

        _jwtServiceMock.Setup(js => js.GetValidRefreshTokenByUserIdAsync(1, CancellationToken.None))!
                       .ReturnsAsync((RefreshToken)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.RefreshJwtToken(CancellationToken.None));
        Assert.Equal("Refresh token not found. Login again", exception.Message);
    }
}
