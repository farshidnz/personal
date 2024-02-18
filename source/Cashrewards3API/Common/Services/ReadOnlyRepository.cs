using Cashrewards3API.Extensions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public interface IReadOnlyRepository
    {
        Task<T> QueryFirstOrDefault<T>(string sql, object parameters = null);

        Task<List<T>> Query<T>(string sql, object parameters = null,int? commandTimeOut =null);

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null);

        Task<IEnumerable<T>> QueryAsyncWithRetry<T>(string sql, object parameters = null);

        Task<(T, List<T1>)> QueryTwoTablesAsync<T, T1>(string sql, object parameters = null);

        Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null) where T : class;

        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string query, Func<TFirst, TSecond, TReturn> map, object param = null,string splitOn = "Id");
    }

    public class ReadOnlyRepository : IReadOnlyRepository
    {
        public ReadOnlyRepository(ShopgoDBContext shopGoDbContext)
        {
            ShopGoDbContext = shopGoDbContext;
        }

        protected ShopgoDBContext ShopGoDbContext { get; }

        protected virtual SqlConnection CreateConnection() => ShopGoDbContext.CreateReadOnlyConnection();

        public async Task<T> QueryFirstOrDefault<T>(string sql, object parameters = null)
        {
            await using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<List<T>> Query<T>(string sql, object parameters = null,int? timeOut =null)
        {
            await using var connection = CreateConnection();
            return (await connection.QueryAsync<T>(sql, parameters,null,timeOut)).ToList();
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null)
        {
            await using var connection = CreateConnection();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<IEnumerable<T>> QueryAsyncWithRetry<T>(string sql, object parameters = null)
        {
            await using var connection = CreateConnection();
            return await connection.QueryAsyncWithRetry<T>(sql, parameters);
        }

        public async Task<(T, List<T1>)> QueryTwoTablesAsync<T, T1>(string sql, object parameters = null)
        {
            await using var connection = CreateConnection();
            using (var multi = await connection.QueryMultipleAsync(sql, parameters))
            {
                var t1 = multi.Read<T>().SingleOrDefault();
                var t2 = multi.Read<T1>().ToList();
                return (t1, t2);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            await using var connection = CreateConnection();
            return await connection.GetAllAsync<T>(transaction, commandTimeout);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string query, Func<TFirst, TSecond, TReturn> map,object param = null,  string splitOn = "Id")
        {
            await using var connection = CreateConnection();
            return await connection.QueryAsync<TFirst, TSecond, TReturn>(query, map, param,null,true,splitOn);
        }
    }
}
