namespace LuminaryLife.Common.Services;

/// <summary>
/// Interface for cache operations
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// Sets a cached value with expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
    
    /// <summary>
    /// Removes a cached value by key
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Removes all cached values matching a pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
}
