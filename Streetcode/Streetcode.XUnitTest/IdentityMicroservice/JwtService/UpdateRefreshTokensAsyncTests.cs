using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Streetcode.Identity.Models.Additional;
using Streetcode.Identity.Models;
using Streetcode.Identity.Repository;
using Streetcode.Identity.Services.Interfaces;
using Microsoft.Extensions.Options;
using Xunit;

using JwtServiceClass = Streetcode.Identity.Services.Realizations.JwtService;

namespace Streetcode.XUnitTest.IdentityMicroservice.JwtService;

public class UpdateRefreshTokensAsyncTests
{
    private readonly Mock<IRefreshRepository> _mockRefreshRepository;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly JsonWebTokenHandler _mockTokenHandler;
    private readonly JwtVariables _jwtVariables;
    private readonly RefreshVariables _refreshVariables;
    private readonly JwtServiceClass _jwtService;

    public UpdateRefreshTokensAsyncTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(storeMock.Object, null, null, null,
                                                                        null, null, null, null, null);
        _mockRefreshRepository = new Mock<IRefreshRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockTokenHandler = new JsonWebTokenHandler();

        _jwtVariables = new JwtVariables
        {
            Secret = "your_secret_key",
            ExpirationInMinutes = 60,
            Issuer = "your_issuer",
            Audience = "your_audience"
        };

        _refreshVariables = new RefreshVariables
        {
            ExpirationInDays = 7
        };

        var jwtOptions = Options.Create(_jwtVariables);
        var refreshOptions = Options.Create(_refreshVariables);

        _jwtService = new JwtServiceClass(
            _mockUserManager.Object,
            _mockRefreshRepository.Object,
            jwtOptions,
            refreshOptions,
            _mockCacheService.Object,
            _mockTokenHandler);
    }

    [Fact]
    public async Task UpdateTokenAsync_ValidToken_ShouldUpdateRefreshToken()
    {
        // Arrange
        var token = new RefreshToken(); 

        // Set up the mock to return a successful save
        _mockRefreshRepository.Setup(repo => repo.Update(token));
        _mockRefreshRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _jwtService.UpdateTokenAsync(token);

        // Assert
        _mockRefreshRepository.Verify(repo => repo.Update(token), Times.Once);
        _mockRefreshRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }


    [Fact]
    public async Task UpdateTokenAsync_SaveChangesFailed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var token = new RefreshToken();
        _mockRefreshRepository.Setup(repo => repo.Update(token));
        _mockRefreshRepository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _jwtService.UpdateTokenAsync(token));
        Assert.Equal("Failed to update refresh token", exception.Message);
    }
}
