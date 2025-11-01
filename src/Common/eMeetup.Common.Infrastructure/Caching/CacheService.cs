using System.Buffers;
using System.Text;
using System.Text.Json;
using eMeetup.Common.Application.Caching;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace eMeetup.Common.Infrastructure.Caching;

internal sealed class CacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _database = redis.GetDatabase();

    // implementation:

    // - /basket/{id} "string" per unique basket
    private static RedisKey BasketKeyPrefix = "/basket/"u8.ToArray();
    // note on UTF8 here: library limitation (to be fixed) - prefixes are more efficient as blobs

    private static RedisKey GetBasketKey(string userId) => BasketKeyPrefix.Append(userId);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        using var data = await _database.StringGetLeaseAsync(GetBasketKey(key));        

        return data is null ? default : JsonSerializer.Deserialize<T>(data.Span);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        byte[] bytes = Serialize(value);
        var created = _database.StringSetAsync(GetBasketKey(key), bytes).Result;

        return Task.CompletedTask;
    }

    private static T Deserialize<T>(byte[] bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes)!;
    }

    private static byte[] Serialize<T>(T value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, value);
        return buffer.WrittenSpan.ToArray();
    }
}
