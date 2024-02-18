using Cashrewards3API.Extensions;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public interface IRepository : IReadOnlyRepository
    {
        Task<int> Execute(string sql, object parameters = null);

        Task<int> ExecuteAsyncWithRetry(string sql, object parameters = null);
    }

    public class Repository : ReadOnlyRepository, IRepository
    {
        public Repository(ShopgoDBContext shopGoDbContext)
            : base(shopGoDbContext)
        {
        }

        protected override SqlConnection CreateConnection() => ShopGoDbContext.CreateConnection();

        public async Task<int> Execute(string sql, object parameters = null)
        {
            await using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<int> ExecuteAsyncWithRetry(string sql, object parameters = null)
        {
            await using var connection = CreateConnection();
            return await connection.ExecuteAsyncWithRetry(sql, parameters);
        }
    }
}
