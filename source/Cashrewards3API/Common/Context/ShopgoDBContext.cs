using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cashrewards3API.Common
{
    public class ShopgoDBContext
    {
        private readonly string _shopgoDbContext;
        private readonly string _shopgoDbReadOnlyContext;

        public ShopgoDBContext(DbConfig dbConfig, IConfiguration configuration)
        {
            if(configuration["Pipeline"] == "DevOps4")
            {
                _shopgoDbContext = $"Data Source={configuration["SQLServerHostWriter"]};Initial Catalog={configuration["ShopGoDatabase"]};user id={configuration["ShopGoUserName"]};password={configuration["ShopGoPassword"]};Max Pool Size=1000;Column Encryption Setting=enabled;ENCRYPT=yes;trustServerCertificate=true";
                _shopgoDbReadOnlyContext = $"Data Source={configuration["SQLServerHostReader"]};Initial Catalog={configuration["ShopGoDatabase"]};user id={configuration["ShopGoUserName"]};password={configuration["ShopGoPassword"]};Max Pool Size=1000;Column Encryption Setting=enabled;ENCRYPT=yes;trustServerCertificate=true";
            } 
            else
            {
                _shopgoDbContext = dbConfig.ShopgoDbContext;
                _shopgoDbReadOnlyContext = dbConfig.ShopgoDbReadOnlyContext;
            }
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_shopgoDbContext);
        }

        public SqlConnection CreateReadOnlyConnection()
        {
            return new SqlConnection(_shopgoDbReadOnlyContext);
        }
    }
}
