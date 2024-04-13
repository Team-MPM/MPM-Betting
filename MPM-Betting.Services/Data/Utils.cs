using Microsoft.Extensions.Caching.Memory;

namespace MPM_Betting.Services.Data;

public partial class Utils
{
    public static T GetViaCache<T>(IMemoryCache cache,  TimeSpan expiration, string cacheKey, Func<T> generator)
    {
        if (cache.TryGetValue(cacheKey, out T? cachedValue))
        {
            return cachedValue!;
        }

        var value = generator.Invoke();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration);

        cache.Set(cacheKey, value, cacheEntryOptions);
        
        return value;
    }
    
    public static async Task<T> GetViaCache<T>(IMemoryCache cache, TimeSpan expiration, string cacheKey, Func<Task<T>> generator)
    {
        if (cache.TryGetValue(cacheKey, out T? cachedValue))
        {
            return cachedValue!;
        }

        var value = await generator.Invoke();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration);

        cache.Set(cacheKey, value, cacheEntryOptions);
        
        return value;
    }
}