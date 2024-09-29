using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.Services.Cache;

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

    public async Task<T> GetAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken = default,
        TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
    {
        T? cachedData = await GetAsync<T>(key, cancellationToken);
        
        if (cachedData is not null)
        {
            return cachedData;
        }
            
        cachedData = await factory();
        
        await SetAsync(key, cachedData, cancellationToken);

        return cachedData;
    }

    public async Task SetAsync<T>(
        string key, 
        T data,
        CancellationToken cancellationToken = default,
        TimeSpan? absoluteExpireTime = null, 
        TimeSpan? unusedExpireTime = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(5),
            SlidingExpiration = unusedExpireTime
        };
        
        string serializedData = JsonConvert.SerializeObject(data);
        await _distributedCache.SetStringAsync(key, serializedData, options, cancellationToken);
    }
}