using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;
using Streetcode.Identity.Models;
using Streetcode.Identity.Models.Additional;
using Streetcode.Identity.Repository;
using Streetcode.Identity.Services.Interfaces;
using Xunit;
using JwtServiceClass = Streetcode.Identity.Services.Realizations.JwtService;

namespace Streetcode.XUnitTest.IdentityMicroservice.JwtService;

public class CreateRefreshTokenAsyncTests
{
    private readonly JwtServiceClass _jwtService;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IRefreshRepository> _refreshRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<JsonWebTokenHandler> _tokenHandlerMock;

    public CreateRefreshTokenAsyncTests()
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
            ExpirationInDays = 7
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
    public async Task CreateRefreshTokenAsync_OnSuccess_ReturnsToken()
    {
        // Arrange
        int userId = 1;
        var user = new ApplicationUser { Id = userId };
        var refreshToken = new RefreshToken { Token = "generated_token", UserId = user.Id, ExpiryDate = DateTime.Now.AddDays(7) };

        _refreshRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                              .ReturnsAsync(refreshToken);
        _refreshRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _cacheServiceMock.Setup(cs => cs.SetAsync<RefreshToken>(
                     $"user-{userId}", It.IsAny<RefreshToken>(), null, null, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);


        // Act
        var result = await _jwtService.CreateRefreshTokenAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(refreshToken.Token, result.Token);
        Assert.Equal(refreshToken.UserId, result.UserId);

        _refreshRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefreshToken>()), Times.Once);
        _refreshRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);

        _cacheServiceMock.Verify(cs => cs.SetAsync<RefreshToken>(
                 $"user-{userId}", It.IsAny<RefreshToken>(), null, null, It.IsAny<CancellationToken>()),
                 Times.Once);
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_OnFail_ThrowsException()
    {
        // Arrange
        int userId = 1;
        var user = new ApplicationUser { Id = userId };
        var refreshToken = new RefreshToken { Token = "generated_token", UserId = user.Id, ExpiryDate = DateTime.Now.AddDays(7) };

        _refreshRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
                              .ReturnsAsync(refreshToken);
        _refreshRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _jwtService.CreateRefreshTokenAsync(user));
        Assert.Equal("Failed to create Refresh token", exception.Message);
    }
}
