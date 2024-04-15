using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace MPM_Betting.Services.Data;

public partial class Utils
{
    // TODO: Implement utility method for getting cached JSON data via external endpoint
    
    public static T GetViaCache<T>(IDistributedCache cache, TimeSpan expiration, string cacheKey, Func<T> generator)
    {
        var cachedValueBytes = cache.Get(cacheKey);
        if (cachedValueBytes != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValueBytes) ?? throw new InvalidOperationException();
        }

        var value = generator.Invoke();
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
        cache.Set(cacheKey, serializedValue, new DistributedCacheEntryOptions { SlidingExpiration = expiration });

        return value;
    }

    public static async Task<T> GetViaCache<T>(IDistributedCache cache, TimeSpan expiration, string cacheKey, Func<Task<T>> generator)
    {
        var cachedValueBytes = await cache.GetAsync(cacheKey);
        if (cachedValueBytes != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValueBytes) ?? throw new InvalidOperationException();
        }

        var value = await generator.Invoke();
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
        await cache.SetAsync(cacheKey, serializedValue, new DistributedCacheEntryOptions { SlidingExpiration = expiration });

        return value;
    }
}