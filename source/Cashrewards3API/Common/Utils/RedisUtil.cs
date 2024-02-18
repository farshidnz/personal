using Cashrewards3API.Common.Context;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Utils
{
    public interface IRedisSemaphore
    {
        Task<bool> WaitAsync(int millisecondsTimeout);
        void Release();
        void StartHealthChecks(CancellationToken stoppingToken);
    }

    public class RedisSemaphore : IRedisSemaphore
    {
        public TimeSpan HealthCheckTimeout { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(10);

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<bool> WaitAsync(int millisecondsTimeout) => await _semaphore.WaitAsync(millisecondsTimeout);

        public void Release() => _semaphore.Release();

        public void StartHealthChecks(CancellationToken stoppingToken)
        {
            Task.Run(async () =>
            {
                Log.Information($"Redis Semaphore health check starting");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await HealthCheck();
                        await Task.Delay(HealthCheckInterval, stoppingToken);
                    }
                    catch (Exception x)
                    {
                        Log.Error($"Redis Semaphore health check error: {x}");
                    }
                }

                Log.Information($"Redis Semaphore health check stopping");
            }, stoppingToken);

        }

        private async Task HealthCheck()
        {
            try
            {
                var locked = await _semaphore.WaitAsync(HealthCheckTimeout);
                if (locked)
                {
                    Log.Information("Redis Semaphore was found to be healthy");
                }
                else
                {
                    Log.Information("Redis Semaphore was found to be stuck and will be released");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public interface IRedisUtil
    {
        Task<T> GetDataAsyncWithEarlyRefresh<T>(string key, Func<Task<T>> cacheMissedAsync, int expiryTime = 30) where T : class;
        Task<T> GetDataAsync<T>(string key, Func<Task<T>> cacheMissedAsync, int expiryTime = 30) where T : class;
        Task<string> GetKeyValueAsync(string key);
        Task<bool> SetKeyValueAsync(string key, string value, int expiryTime = 30);
    }

    public class RedisUtil : IRedisUtil
    {
        private readonly IDatabase _redisDb;
        private readonly ILogger<IRedisUtil> _logger;
        private readonly IRedisSemaphore _redisSemaphore;
        private readonly CacheConfig _cacheConfig;

        public RedisUtil(IDatabase database, ILogger<IRedisUtil> logger, IRedisSemaphore redisSemaphore, CacheConfig cacheConfig)
        {
            _redisDb = database;
            _logger = logger;
            _redisSemaphore = redisSemaphore;
            _cacheConfig = cacheConfig;
        }

        public async Task<T> GetDataAsyncWithEarlyRefresh<T>(string key, Func<Task<T>> cacheMissedAsync, int expiryTime = 30) where T : class
        {
            var timeToLive = await _redisDb.KeyTimeToLiveAsync(key);
            if (!timeToLive.HasValue || timeToLive.Value < (TimeSpan.FromSeconds(expiryTime) * (_cacheConfig.EarlyCacheRefreshPercentage * 0.01)))
            {
                if (await _redisSemaphore.WaitAsync(0))
                {
                    try
                    {
                        _logger.LogWarning($"Refreshing cache early. Key: {key}");
                        return await MissedCached(key, cacheMissedAsync, expiryTime);
                    }
                    finally
                    {
                        _redisSemaphore.Release();
                    }
                }
            }

            return await GetDataAsync(key, cacheMissedAsync, expiryTime);
        }

        public async Task<T> GetDataAsync<T>(string key, Func<Task<T>> cacheMissedAsync, int expiryTime = 30) where T : class
        {
            RedisValue responseFromCache = default;
            try
            {   
                responseFromCache = await _redisDb.StringGetAsync(key);
                
                if (responseFromCache != RedisValue.Null)
                {
                    _logger.LogInformation($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetData Response received with data from Cache for key : {key}");
                    return JsonConvert.DeserializeObject<T>(responseFromCache);
                }
                else
                    _logger.LogInformation(
                        $"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetData Response missed for key : {key}");
                   
                
            }
            catch (Exception e)
            {   
                _logger.LogCritical($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetDataAsync_StringGetAsyncc Exception : {e.Message}");
            }
           
            _logger.LogInformation($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetData Response Key : {key} not found in Cache, fetching from DB.");

            _logger.LogWarning($"Missed cache. Key: {key}");
            return await MissedCached(key, cacheMissedAsync, expiryTime);
        }

        private async Task<T> MissedCached<T>(string key, Func<Task<T>> cacheMissedAsync, int expiryTime)
        {
            var responseFromDb = await cacheMissedAsync();
            if (responseFromDb != null)
            {
                try
                {
                    var result = await _redisDb.StringSetAsync(key, JsonConvert.SerializeObject(responseFromDb),
                        TimeSpan.FromSeconds(expiryTime));
                }
                catch (Exception e)
                {
                    _logger.LogCritical($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetDataAsync_StringGetAsyncc Executing cacheMissedAsync Exception : {e.Message}");
                }
            }

            return responseFromDb;
        }


        public async Task<string> GetKeyValueAsync(string key)
        {
            RedisValue responseFromCache = default;
            try
            {
                // TODO: need to implement Polly here for retry pattern.
                responseFromCache = await _redisDb.StringGetAsync(key);
                if (responseFromCache != RedisValue.Null)
                {
                    _logger.LogInformation($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetKeyValue Response received with data from Cache for key : {key}.");
                    return responseFromCache;
                }
                else
                {
                    _logger.LogInformation($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetKeyValue Response missed 1st for key : {key}.");
                    responseFromCache = await _redisDb.StringGetAsync(key);
                    if (responseFromCache != RedisValue.Null)
                    {
                        _logger.LogInformation($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetKeyValue Response received with data from Cache for key : {key}.");
                        return responseFromCache;
                    }
                    else
                        _logger.LogInformation(
                            $"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetKeyValue Response received with no data from Cache for key : {key}");
                }

            }
            catch (Exception e)
            {   
                _logger.LogCritical($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : GetKeyValue Exception for key : {key} : Exception : {e.Message}");
            }

            return null;
        }

        public async Task<bool> SetKeyValueAsync(string key, string value, int expiryTime = 30)
        {
            try
            {
                return await _redisDb.StringSetAsync(key, value, TimeSpan.FromSeconds(expiryTime));
            }
            catch (Exception e)
            {
                _logger.LogCritical($"CorrelationId :  {CorrelationContext.GetCorrelationId()} : SetKeyValue Exception : {e.Message}");
            }
            return false;

        }
    }
}
