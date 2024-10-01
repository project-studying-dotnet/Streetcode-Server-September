namespace Streetcode.Identity.Services.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpireTime = null,
        TimeSpan? unusedExpireTime = null, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
