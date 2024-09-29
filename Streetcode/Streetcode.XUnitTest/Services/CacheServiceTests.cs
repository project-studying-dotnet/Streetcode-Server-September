using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Services.Cache;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Xunit;

namespace Streetcode.XUnitTest.Services;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _distributedCache;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _distributedCache = new Mock<IDistributedCache>();
        _cacheService = new CacheService(_distributedCache.Object);
    }

    [Fact]
    public async Task GetAsyncNullable_ReturnsDefaultValue_WhenCacheMiss()
    {
        // Arrange
        _distributedCache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<object>("key", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetAsyncNullable_ThrowsCustomException_WhenDeserializationFails()
    {
        // Arrange
        _distributedCache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var exception = await Assert.ThrowsAsync<CustomException>(
            () => _cacheService.GetAsync<object>("key", CancellationToken.None));

        // Assert
        Assert.Equal("Error while deserializing cached value", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
    }
    
    [Fact]
    public async Task GetAsyncNullable_ReturnsData_WhenCacheHitAnfDeserializationSuccessful()
    {
        // Arrange
        var expectedObject = new Text { Title = "Value" };
        var serializedData = JsonConvert.SerializeObject(expectedObject);
        var byteArray = Encoding.UTF8.GetBytes(serializedData);

        _distributedCache
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(byteArray);

        // Act
        var result = await _cacheService.GetAsync<Text>("key", CancellationToken.None);

        // Assert
        result?.Title.Should().Be(expectedObject.Title);
    }

    [Fact]
    public async Task GetAsyncNonNullable_ReturnsCachedDataInstantly_WhenCacheHit()
    {
        // Arrange
        var expectedObject = new Text { Title = "Value" };
        var serializedData = JsonConvert.SerializeObject(expectedObject);
        var byteArray = Encoding.UTF8.GetBytes(serializedData);

        _distributedCache
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(byteArray);
        
        // Act
        var result = await _cacheService.GetAsync("key", It.IsAny<Func<Task<Text>>>());
        
        // Assert
        result.Title.Should().Be(expectedObject.Title);
        _distributedCache.Verify(
            cache => cache.SetAsync(
                It.IsAny<string>(), 
                It.IsAny<byte[]>(), 
                It.IsAny<DistributedCacheEntryOptions>(), 
                It.IsAny<CancellationToken>()),
            Times.Never());
    }
    
    [Fact]
    public async Task GetAsyncNonNullable_ReturnsCachedDataAfterPopulatingCache_WhenCacheMiss()
    {
        // Arrange
        var expectedObject = new Text { Title = "Value" };
        Task<Text> Factory() => Task.Run(() => expectedObject);

        _distributedCache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        
        // Act
        var result = await _cacheService.GetAsync("key", Factory);
        
        // Assert
        result.Title.Should().Be(expectedObject.Title);
        _distributedCache.Verify(
            cache => cache.SetAsync(
                It.IsAny<string>(), 
                It.IsAny<byte[]>(), 
                It.IsAny<DistributedCacheEntryOptions>(), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
       [Fact]
    public async Task SetAsync_SavesSerializedData_WhenCalled()
    {
        // Arrange
        var key = "key";
        var data = new Text { Title = "Value" };
        var expectedJson = JsonConvert.SerializeObject(data);
        var bytes = Encoding.UTF8.GetBytes(expectedJson);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        // Act
        await _cacheService.SetAsync(key, data);

        // Assert
        _distributedCache
            .Verify(x => x.SetAsync(key, bytes, 
                It.Is<DistributedCacheEntryOptions>(o => 
                    o.AbsoluteExpirationRelativeToNow == options.AbsoluteExpirationRelativeToNow), 
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_UsesCustomExpirationTimes_WhenProvided()
    {
        // Arrange
        var key = "key";
        var data =new Text { Title = "Value" };
        var absoluteExpireTime = TimeSpan.FromMinutes(10);
        var unusedExpireTime = TimeSpan.FromMinutes(2);
        var expectedJson = JsonConvert.SerializeObject(data);
        var bytes = Encoding.UTF8.GetBytes(expectedJson);
        
        // Act
        await _cacheService.SetAsync(key, data, absoluteExpireTime, unusedExpireTime, CancellationToken.None);

        // Assert
        _distributedCache
            .Verify(x => x.SetAsync(key, bytes, 
                It.Is<DistributedCacheEntryOptions>(o => 
                    o.AbsoluteExpirationRelativeToNow == absoluteExpireTime &&
                    o.SlidingExpiration == unusedExpireTime), 
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_UsesDefaultExpiration_WhenNotProvided()
    {
        // Arrange
        var key = "key";
        var data = new Text { Title = "Value" };
        var expectedJson = JsonConvert.SerializeObject(data);
        var bytes = Encoding.UTF8.GetBytes(expectedJson);

        // Act
        await _cacheService.SetAsync(key, data);

        // Assert
        _distributedCache
            .Verify(x => x.SetAsync(key, bytes, 
                It.Is<DistributedCacheEntryOptions>(o => 
                    o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(5) &&
                    o.SlidingExpiration == null), 
                It.IsAny<CancellationToken>()), Times.Once);
    }
}