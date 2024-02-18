using Cashrewards3API.Common;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Common.Util
{
    public class RedisUtilTests
    {
        private class TestState
        {
            public RedisUtil RedisUtil { get; }

            public IDatabase Database { get; }
            public ConcurrentDictionary<string, (TimeSpan? expiry, string value)> Cache { get; set; }

            public int MissedCacheCalls { get; private set; }

            public TestState()
            {
                (Database, Cache) = RedisDatabaseMockFactory.Create();
                RedisUtil = new RedisUtil(Database, new ConsoleLoggerMock<IRedisUtil>(LogLevel.Warning).Object, new RedisSemaphore(), new CacheConfig { EarlyCacheRefreshPercentage = 80 });
            }

            public async Task<CacheData> MissedCache()
            {
                IncCacheMissCount();
                await Task.Delay(10);
                return await Task.FromResult(new CacheData { Value = "value" });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void IncCacheMissCount() => MissedCacheCalls++;

            public void GivenCacheDataExistsForKey(string key, TimeSpan? expiry = null, CacheData value = null)
            {
                Cache[key] = (expiry ?? TimeSpan.FromSeconds(30), JsonConvert.SerializeObject(value ?? new CacheData { Value = "value" }));
            }
        }

        private class CacheData
        {
            public string Value { get; set; }
        }

        [Test]
        public async Task GetDataAsyncWithEarlyRefresh_ShouldUseCache_GivenCacheDataExists()
        {
            var state = new TestState();
            state.GivenCacheDataExistsForKey("key", TimeSpan.FromSeconds(30), new CacheData { Value = "abcd" });

            var tasks = new Task[]
            {
                Task.Run(async () => { await Task.Delay(100); (await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30)).Value.Should().Be("abcd"); }),
                Task.Run(async () => { await Task.Delay(100); (await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30)).Value.Should().Be("abcd"); }),
                Task.Run(async () => { await Task.Delay(200); (await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30)).Value.Should().Be("abcd"); }),
                Task.Run(async () => { await Task.Delay(200); (await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30)).Value.Should().Be("abcd"); }),
                Task.Run(async () => { await Task.Delay(200); (await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30)).Value.Should().Be("abcd"); }),
                Task.Run(async () => { await Task.Delay(200); (await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30)).Value.Should().Be("abcd"); }),
                Task.Run(async () => { await Task.Delay(200); (await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30)).Value.Should().Be("abcd"); })
            };
            await Task.WhenAll(tasks);
        }

        [Test]
        public async Task GetDataAsyncWithEarlyRefresh_ShouldNotRefreshCacheEarly_GivenCacheIsLongTimeToExpiry()
        {
            var state = new TestState();
            state.GivenCacheDataExistsForKey("key", TimeSpan.FromSeconds(30));

            await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30);

            state.MissedCacheCalls.Should().Be(0);
        }

        [Test]
        public async Task GetDataAsyncWithEarlyRefresh_ShouldRefreshCacheEarly_GivenCacheIsNearlyExpired()
        {
            var state = new TestState();
            state.GivenCacheDataExistsForKey("key", TimeSpan.FromSeconds(2));

            await state.RedisUtil.GetDataAsyncWithEarlyRefresh("key", state.MissedCache, 30);

            state.MissedCacheCalls.Should().Be(1);
        }

        [Test]
        public async Task StartHealthChecks_ShouldReleaseTheSemaphoreWhenSeeminglyLockedForever()
        {
            var semaphore = new RedisSemaphore
            {
                HealthCheckInterval = TimeSpan.FromMilliseconds(10),
                HealthCheckTimeout = TimeSpan.FromMilliseconds(50),
            };
            var tokenSource = new CancellationTokenSource();
            semaphore.StartHealthChecks(tokenSource.Token);

            var isLocked = await semaphore.WaitAsync(0);
            isLocked.Should().BeTrue();

            await Task.Delay(1000);
            var isLockedAgain = await semaphore.WaitAsync(0);
            isLockedAgain.Should().BeTrue();

            tokenSource.Cancel();
        }
    }
}
