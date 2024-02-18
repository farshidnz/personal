using ImpromptuInterface;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Helpers
{
    public class RedisDatabaseMockFactory
    {
        private class RedisDatabaseMock
        {
            public ConcurrentDictionary<string, (TimeSpan? expiry, string value)> Cache { get; } = new();

            public Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key, CommandFlags flags = CommandFlags.None) => Task.FromResult(Cache.TryGetValue(key, out var v) ? v.expiry : null);

            public Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None) => Task.FromResult(new RedisValue(Cache.TryGetValue(key, out var v) ? v.value : null));

            public Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
            {
                Cache[key] = (expiry, value);
                return Task.FromResult(true);
            }
        }

        public static (IDatabase, ConcurrentDictionary<string, (TimeSpan? expiry, string value)>) Create()
        {
            var database = new RedisDatabaseMock();
            return (database.ActLike<IDatabase>(), database.Cache);
        }
    }
}
