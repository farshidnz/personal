using Cashrewards3API.Middlewares;
using Dapper;
using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Cashrewards3API.Extensions
{
    public static class DapperExtension
    {
        private static readonly IEnumerable<TimeSpan> RetryTimes = new[]
    {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            };

        private static readonly AsyncRetryPolicy RetryPolicy = Policy
                                                         .Handle<SqlException>(SqlServerTransientExceptionDetector.ShouldRetryOn)
                                                         .Or<TimeoutException>()
                                                         .WaitAndRetryAsync(RetryTimes,
                                                                        (exception, timeSpan, retryCount, context) =>
                                                                        {
                                                                            var log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
                                                                            log.Warning(exception, $"WARNING: Error ShopGo DB, will retry after {timeSpan}. Retry attempt {retryCount}");
                                                                        });

        public static async Task<int> ExecuteAsyncWithRetry(this IDbConnection cnn, string sql, object param = null,
                                                        IDbTransaction transaction = null, int? commandTimeout = null,
                                                        CommandType? commandType = null) =>
                             await RetryPolicy.ExecuteAsync(async () => await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));

        public static async Task<IEnumerable<T>> QueryAsyncWithRetry<T>(this IDbConnection cnn, string sql, object param = null,
                                                                IDbTransaction transaction = null, int? commandTimeout = null,
                                                                CommandType? commandType = null) =>
                             await RetryPolicy.ExecuteAsync(async () => await cnn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType));
    }
}