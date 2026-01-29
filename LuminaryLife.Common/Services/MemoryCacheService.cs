using System.Collections.Concurrent;
using System.Text.Json;

namespace LuminaryLife.Common.Services;

/// <summary>
/// In-memory cache implementation for development/testing
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    private class CacheEntry
    {
        public string Value { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Expiration > DateTime.UtcNow)
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(entry.Value));
            }
            
            _cache.TryRemove(key, out _);
        }
        
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var entry = new CacheEntry
        {
            Value = JsonSerializer.Serialize(value),
            Expiration = DateTime.UtcNow.Add(expiration)
        };
        
        _cache.AddOrUpdate(key, entry, (_, _) => entry);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        var regex = new System.Text.RegularExpressions.Regex(
            "^" + pattern.Replace("*", ".*") + "$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        var keysToRemove = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();
        
        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }
        
        return Task.CompletedTask;
    }
}
