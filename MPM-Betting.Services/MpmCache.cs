using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MPM_Betting.Services;

public class MpmCache(IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<MpmCache> logger)
{
    public T GetViaCache<T>(TimeSpan expiration, string cacheKey, Func<T> generator)
    {
        var cachedValueBytes = distributedCache.Get(cacheKey);
        if (cachedValueBytes != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValueBytes) ?? throw new InvalidOperationException();
        }

        var value = generator.Invoke();
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
        distributedCache.Set(cacheKey, serializedValue, new DistributedCacheEntryOptions { SlidingExpiration = expiration });

        return value;
    }

    public async Task<T> GetViaCache<T>(TimeSpan expiration, string cacheKey, Func<Task<T>> generator)
    {
        var cachedValueBytes = await distributedCache.GetAsync(cacheKey);
        if (cachedValueBytes != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValueBytes) ?? throw new InvalidOperationException();
        }

        var value = await generator.Invoke();
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
        await distributedCache.SetAsync(cacheKey, serializedValue, new DistributedCacheEntryOptions { SlidingExpiration = expiration });

        return value;
    }
}