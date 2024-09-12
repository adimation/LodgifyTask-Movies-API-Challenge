using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Extensions
{
    public static class DistributedCacheExtensions
    {
        // Set an object in the cache
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options = null)
        {
            if (options == null)
                options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)).SetAbsoluteExpiration(TimeSpan.FromHours(1));

            var json = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key, json, options);
        }

        // Get an object from the cache
        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        {
            var json = await cache.GetStringAsync(key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }
    }
}
