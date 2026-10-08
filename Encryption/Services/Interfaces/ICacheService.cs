namespace Encryption.Services.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<bool> IncrementRateLimitAsync(string key, TimeSpan window, int maxPermits);

    }
}
