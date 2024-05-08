using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using LanguageExt.Common;

namespace MPM_Betting.Services;

public class MpmCacheException() : Exception("Cache error");

public class MpmCache(
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    ILogger<MpmCache> logger,
    HttpClient httpClient)
{
    public async Task<T> Get<T>(TimeSpan expiration, string cacheKey, Func<Task<T>> generator)
    {
        if (memoryCache.TryGetValue(cacheKey, out T? cachedValue))
        {
            return cachedValue!;
        }

        logger.LogInformation("Cache miss for {CacheKey}", cacheKey);
        var value = await generator.Invoke();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration);

        memoryCache.Set(cacheKey, value, cacheEntryOptions);

        return value;
    }

    public T Get<T>(TimeSpan expiration, string cacheKey, Func<T> generator)
    {
        if (memoryCache.TryGetValue(cacheKey, out T? cachedValue))
        {
            return cachedValue!;
        }

        logger.LogInformation("Cache miss for {CacheKey}", cacheKey);
        var value = generator.Invoke();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration);

        memoryCache.Set(cacheKey, value, cacheEntryOptions);

        return value;
    }

    public async Task<MpmResult<T>> GetPersistent<T>(TimeSpan expiration, string cacheKey, Func<Task<T>> generator)
    {
        var cachedValueBytes = await distributedCache.GetAsync(cacheKey);
        if (cachedValueBytes != null)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(cachedValueBytes) ?? throw new InvalidOperationException();
            }
            catch
            {
                return new MpmCacheException();
            }
        }

        logger.LogInformation("Cache miss for {CacheKey}", cacheKey);
        var value = await generator.Invoke();
        var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
        await distributedCache.SetAsync(cacheKey, serializedValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });

        return value;
    }

    public async Task<MpmResult<string>> GetByUri(TimeSpan expiration, Uri uri)
    {
        try
        {
            var cacheValueBytes = await distributedCache.GetAsync(uri.ToString());
            if (cacheValueBytes is not null)
            {
                return Encoding.UTF8.GetString(cacheValueBytes);
            }

            logger.LogInformation("Cache miss for {Uri}", uri);
            var response = await httpClient.GetAsync(uri);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new InvalidOperationException();
            }

            var value = await response.Content.ReadAsStringAsync();
            var byteValue = Encoding.UTF8.GetBytes(value);
            await distributedCache.SetAsync(uri.ToString(), byteValue, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration
            });

            return value;
        }
        catch
        {
            return new MpmCacheException();
        }
    }
}