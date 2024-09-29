namespace Streetcode.BLL.Services.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task<T> GetAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken = default,
        TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null);

    Task SetAsync<T>(string key, T data, CancellationToken cancellationToken = default, 
        TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null);
}