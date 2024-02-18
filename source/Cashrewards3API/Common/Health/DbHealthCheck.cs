using System;
using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Health
{
    public class DbHealthCheck : IHealthCheck
    {
        private readonly string connectionString;

        public DbHealthCheck(IConfiguration configuration)
        {
            if (configuration["Pipeline"] == "DevOps4")
            {
                connectionString = $"Data Source={configuration["SQLServerHostWriter"]};Initial Catalog={configuration["ShopGoDatabase"]};user id={configuration["ShopGoUserName"]};password={configuration["ShopGoPassword"]};Max Pool Size=1000;Column Encryption Setting=enabled;ENCRYPT=yes;trustServerCertificate=true";
            }
            else
            {
                connectionString = configuration["ConnectionStrings:ShopgoDbContext"];
            }
        }
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            const string sql = "SELECT 1";
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.QuerySingleAsync(sql);

                return HealthCheckResult.Healthy("Ok");
            }
            catch (Exception ex)
            {

                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}