using System.Text.Json;
using Encryption.Services.Interfaces;
using StackExchange.Redis;

namespace Encryption.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string RateLimitScript = @"
        local current = redis.call('INCR', @key)
        if current == 1 then
            redis.call('PEXPIRE', @key, @expiry)
        end
        return current";

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            RedisValue value = await _database.StringGetAsync(key);

            if (!value.HasValue)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
        }
        catch (RedisException)
        {
            return default;
        }

    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            string json = JsonSerializer.Serialize(value, JsonOptions);
            var expiration = expiry.HasValue ? (Expiration)expiry.Value : Expiration.Default;
            await _database.StringSetAsync(key, json, expiration);
        }
        catch (RedisException)
        {
        }
        
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }

        catch (RedisException)
        {

        }
    }

    public async Task<bool> IncrementRateLimitAsync(string key, TimeSpan window, int maxPermits)
    {
        try
        {
            var script = LuaScript.Prepare(RateLimitScript);
            var result = await _database.ScriptEvaluateAsync(script, new
            {
                key = (RedisKey)key,
                expiry = (long)window.TotalMilliseconds
            });

            long currentCount = (long)result;
            return currentCount <= maxPermits;
        }
        catch (RedisException)
        {
            return true;
        }
        
    }
}