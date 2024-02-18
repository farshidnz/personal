using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Utils;

namespace Cashrewards3API.Common.Services
{
    public interface ICrApplicationKeyValidationService
    {
        bool IsValid(string key);
    }

    public class CrApplicationKeyValidationService : ICrApplicationKeyValidationService
    {
        private readonly IConfiguration _configuration;
        private readonly IRedisUtil _redisUtil;
        private readonly ICacheKey _cacheKey;
        private readonly CacheConfig _cacheConfig;
        private const string MAIN_SITE_API_KEY_NAME = "AWS:MainSiteApiKeyName";

        public CrApplicationKeyValidationService(IConfiguration configuration, 
            IRedisUtil redisUtil,
            ICacheKey cacheKey,
            CacheConfig cacheConfig)
        {
            _configuration = configuration;
            _redisUtil = redisUtil;
            _cacheKey = cacheKey;
            _cacheConfig = cacheConfig;
        }

        private char APPLICATION_KEY_SEPERATOR = ',';

        public bool IsValid(string key)
        {
            var systemKeys = GetCrApplication().Key?.Split(APPLICATION_KEY_SEPERATOR);
            return key.Split(APPLICATION_KEY_SEPERATOR).Intersect(systemKeys!).Any();
        }

        private  CrApplication GetCrApplication()
        {
            var key = _cacheKey.GetCrApplicationKey(_configuration[MAIN_SITE_API_KEY_NAME]);
            var value = _redisUtil.GetDataAsync<CrApplication>(key,
                () => GetApplicationSettingsAsync(),
                _cacheConfig.CrApplicationKeyExpiry).ConfigureAwait(false).GetAwaiter().GetResult();

            return value;
        }

        private async Task<CrApplication> GetApplicationSettingsAsync()
        {
            var client = new AmazonSimpleSystemsManagementClient();

            var parameter = new GetParameterRequest { Name = _configuration[MAIN_SITE_API_KEY_NAME], WithDecryption = true };

            var resultResponse = client.GetParameterAsync(parameter).ConfigureAwait(false).GetAwaiter().GetResult();
            var key = resultResponse?.Parameter?.Value;
            return new CrApplication(key);
        }
    }
}