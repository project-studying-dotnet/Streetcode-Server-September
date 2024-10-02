using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Streetcode.Identity.Exceptions;
using Streetcode.Identity.Services.Interfaces;

namespace Streetcode.Identity.Services.Realizations;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        string? data = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (data is null)
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(data) ?? throw new CustomException(
            "Error while deserializing cached value",
            StatusCodes.Status500InternalServerError);
    }

    public async Task<T> GetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null,
        CancellationToken cancellationToken = default)
    {
        T? cachedData = await GetAsync<T>(key, cancellationToken);

        if (cachedData is not null)
        {
            return cachedData;
        }

        cachedData = await factory();

        await SetAsync(key, cachedData, cancellationToken: cancellationToken);

        return cachedData;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    public async Task SetAsync<T>(
        string key,
        T data,
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromDays(7),
            SlidingExpiration = unusedExpireTime
        };

        string serializedData = JsonConvert.SerializeObject(data);
        await _distributedCache.SetStringAsync(key, serializedData, options, cancellationToken);
    }
}
