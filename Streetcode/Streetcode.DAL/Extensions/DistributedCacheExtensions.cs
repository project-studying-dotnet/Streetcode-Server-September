using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Streetcode.DAL.Extensions;

public static class DistributedCacheExtensions
{
    public static async Task SetRecordAsync<T>(
        this IDistributedCache distributedCache,
        string recordId, 
        T data,
        TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(10),
            SlidingExpiration = unusedExpireTime
        };

        var jsonData = JsonConvert.SerializeObject(data);
        await distributedCache.SetStringAsync(recordId, jsonData, options);
    }

    public static async Task<T?> GetRecordAsync<T>(this IDistributedCache distributedCache, string recordId)
    {
        var jsonData = await distributedCache.GetStringAsync(recordId);
        
        if (jsonData is null)
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(jsonData) 
               ?? throw new InvalidOperationException($"Can not deserialize {typeof(T)}");
    }
}