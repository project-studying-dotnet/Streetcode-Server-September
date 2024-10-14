using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Streetcode.Identity.Models.Additional;
using Streetcode.Identity.Models;
using Streetcode.Identity.Repository;
using Xunit;
using Streetcode.Identity.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace Streetcode.XUnitTest.IdentityMicroservice.JwtService;

using JwtServiceClass = Streetcode.Identity.Services.Realizations.JwtService;

public class DeleteRefreshTokensAsyncTests
{
    private readonly Mock<IRefreshRepository> _mockRefreshRepository;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly JsonWebTokenHandler _mockTokenHandler;
    private readonly JwtVariables _jwtVariables;
    private readonly RefreshVariables _refreshVariables;
    private readonly JwtServiceClass _jwtService;

    public DeleteRefreshTokensAsyncTests()
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
    public async Task DeleteInvalidTokensAsync_RemovesInvalidTokens_Success()
    {
        // Arrange
        var invalidTokens = new List<RefreshToken>
        {
            new RefreshToken { IsRevoked = true, ExpiryDate = DateTime.Now.AddDays(-1) },
            new RefreshToken { IsRevoked = false, ExpiryDate = DateTime.Now.AddDays(-1) }
        };

        _mockRefreshRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(invalidTokens);

        _mockRefreshRepository
           .Setup(repo => repo.SaveChangesAsync())
           .ReturnsAsync(1);

        // Act
        await _jwtService.DeleteInvalidTokensAsync();

        // Assert
        _mockRefreshRepository.Verify(repo => repo.Delete(invalidTokens), Times.Once);
        _mockRefreshRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteInvalidTokensAsync_SaveChangesFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidTokens = new List<RefreshToken>
        {
            new RefreshToken { IsRevoked = true, ExpiryDate = DateTime.Now.AddDays(-1) }
        };

        _mockRefreshRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>()))
            .ReturnsAsync(invalidTokens);

        _mockRefreshRepository
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _jwtService.DeleteInvalidTokensAsync());

        Assert.Equal("Failed to clear invalid tokens", exception.Message);
    }

}
