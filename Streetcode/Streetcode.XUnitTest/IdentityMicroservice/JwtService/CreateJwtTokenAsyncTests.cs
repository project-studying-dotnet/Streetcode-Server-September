using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Streetcode.Identity.Models.Additional;
using Streetcode.Identity.Models;
using Streetcode.Identity.Repository;
using Streetcode.Identity.Services.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using Moq;

using JwtServiceClass = Streetcode.Identity.Services.Realizations.JwtService;

namespace Streetcode.XUnitTest.IdentityMicroservice.JwtService;

public class CreateJwtTokenAsyncTests
{
    private readonly JwtServiceClass _jwtService;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IRefreshRepository> _refreshRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<JsonWebTokenHandler> _tokenHandlerMock;

    public CreateJwtTokenAsyncTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null, null, null,
                                                                        null, null, null, null, null);

        var jwtVariables = Options.Create(new JwtVariables
        {
            Secret = "super_secret_reliable_key_long_enough",
            ExpirationInMinutes = 60,
            Issuer = "your-issuer",
            Audience = "your-audience"
        });
        var refreshVariables = Options.Create(new RefreshVariables
        {
            ExpirationInDays = 30
        });

        _refreshRepositoryMock = new Mock<IRefreshRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _tokenHandlerMock = new Mock<JsonWebTokenHandler>();

        _jwtService = new JwtServiceClass(
            _userManagerMock.Object,
            _refreshRepositoryMock.Object,
            jwtVariables,
            refreshVariables,
            _cacheServiceMock.Object,
            _tokenHandlerMock.Object
        );
    }

    [Fact]
    public async Task CreateJwtTokenAsync_OnSuccess_ReturnsToken()
    {
        // Arrange
        int userId = 1;
        var user = new ApplicationUser { Id = userId };
        var roles = new List<string> { "Admin", "User" };

        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
        _tokenHandlerMock.Setup(th => th.CreateToken(It.IsAny<SecurityTokenDescriptor>())).Returns("generated_token");

        // Act
        var token = await _jwtService.CreateJwtTokenAsync(user);

        // Assert
        Assert.Equal("generated_token", token);
        _userManagerMock.Verify(um => um.GetRolesAsync(user), Times.Once);
        _tokenHandlerMock.Verify(th => th.CreateToken(It.IsAny<SecurityTokenDescriptor>()), Times.Once);
    }

    [Fact]
    public async Task CreateJwtTokenAsync_TokenGenerationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        int userId = 1;
        var user = new ApplicationUser { Id = userId };
        var roles = new List<string> { "User" };

        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(roles);
        _tokenHandlerMock.Setup(th => th.CreateToken(It.IsAny<SecurityTokenDescriptor>())).Returns<string>(null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _jwtService.CreateJwtTokenAsync(user));
        Assert.Equal("Token generation failed", exception.Message);
    }

}