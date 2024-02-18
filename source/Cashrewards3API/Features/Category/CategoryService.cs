using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Dapper;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Category.Interface;

namespace Cashrewards3API.Features.Category
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(int clientId, int? premiumClientId, Status status);

        Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int clientId, int? premiumClientId, int rootCategoryId, Status status);

        Task<IEnumerable<CategoryDto>> GetNonRootCategoriesAsync(Status status, CategoryTypeEnum categoryType);

        Task<IEnumerable<CategoryDto>> GetCategoriesByMerchantIdAsync(int merchanteId);

        Task<IEnumerable<CategoryWithCountDTO>> GetRootCategoriesWithCountAsync(int clientId, int? premiumClientId, Status status);

    }

    public class CategoryService : ICategoryService
    {
        private readonly IConfiguration Configuration;
        private readonly IRedisUtil redisUtil;
        private readonly ICacheKey cacheKey;
        private readonly CacheConfig cacheConfig;
        private readonly ICategoryRepository _categoryRepository;
        
        public CategoryService(IConfiguration configuration,
                            IRedisUtil redisUtil,
                            ICacheKey cacheKey,
                            CacheConfig cacheConfig,
                            ICategoryRepository categoryRepository
            )
        {
            Configuration = configuration;
            this.redisUtil = redisUtil;
            this.cacheKey = cacheKey;
            this.cacheConfig = cacheConfig;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync(int clietnId, int? premiumClientId , Status status)
        {
            string key = cacheKey.GetRootCategoriesCacheKey(clietnId, status);
            return await redisUtil.GetDataAsync(key,
                                    () => GetRootCategoriesFromDbAsync(clietnId, premiumClientId, status), cacheConfig.CategoryDataExpiry);

        }

        public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int clietnId, int? premiumClientId, int rootCategoryId, Status status)
        {
            string key = cacheKey.GetSubCategoriesCacheKey(clietnId, rootCategoryId, status);
            return await redisUtil.GetDataAsync(key,
                () => GetSubCategoriesFromDbAsync(clietnId, premiumClientId, rootCategoryId, status), cacheConfig.CategoryDataExpiry);
        }

        public async Task<IEnumerable<CategoryDto>> GetNonRootCategoriesAsync(Status status, CategoryTypeEnum categoryType)
        {
            var categories = await _categoryRepository.GetNonRootCategoriesAsync(status, categoryType);
            return ConvertToDto(categories);
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesByMerchantIdAsync(int merchantId)
        {
            var categories = await _categoryRepository.GetCategoriesByMerchantIdAsync(merchantId);
            return ConvertToDto(categories);
        }

        private async Task<IEnumerable<CategoryDto>> GetRootCategoriesFromDbAsync(int clientId, int? premiumClientId, Status status)
        {
            var clientIds = GetClientIds(clientId, premiumClientId);
            var categories = await GetCategoriesAsync(clientIds, status);
            return ConvertToDto(categories.Where(x => x.RootCategoryId == null));
        }

        private async Task<IEnumerable<CategoryDto>> GetSubCategoriesFromDbAsync(int clietnId, int? premiumClientId, int rootCategoryId, Status status)
        {
            var clientIds = GetClientIds(clietnId, premiumClientId);
            var categories = await GetCategoriesAsync(clientIds, status);

            return ConvertToDto(categories.Where(x => x.RootCategoryId == rootCategoryId));
        }

        private async Task<IEnumerable<CategoryModel>> GetCategoriesAsync(List<int> clientIds, Status status)
        {
            IEnumerable<CategoryModel> categories;
            if (status == Status.All)
            {
                categories = await _categoryRepository.GetCategoriesByClientIdAsync(clientIds);
            }
            else
            {
                categories = await _categoryRepository.GetCategoriesByClientIdAndStatusAsync(clientIds, status);
            }

            return categories.Distinct(new CategoryByIdComparer());
        }

        private List<int> GetClientIds(int clientId, int? premiumClientId)
        {
            List<int> clieIntIds = new List<int> {clientId};
            if (premiumClientId.HasValue)
                clieIntIds.Add(premiumClientId.Value);

            return clieIntIds;
        }

        
        public async Task<IEnumerable<CategoryWithCountDTO>> GetRootCategoriesWithCountAsync(int clientId, int? premiumClientId, Status status)
        {
            var clientIds = GetClientIds(clientId, premiumClientId);
            var categories = (await _categoryRepository.GetRootCategoriesByClientIdAndStatusWithCountsAsync(clientIds, status))
                .OrderByDescending(c => c.Ranking);

            return ConvertToExtendedDto(categories.Where(x => x.RootCategoryId == null));
        }

        
        private static IEnumerable<CategoryDto> ConvertToDto(IEnumerable<CategoryModel> categories)
        {
            return categories
                .Select(x => new CategoryDto
                {
                    Id = x.CategoryId,
                    Status = x.Status,
                    HyphenatedString = x.HyphenatedString,
                    MetaDescription = x.MetaDescription,
                    Name = x.Name
                });
        }

        private static IEnumerable<CategoryWithCountDTO> ConvertToExtendedDto(IEnumerable<CategoryModel> categories)
        {
            return categories
                .Select(x => new CategoryWithCountDTO
                {
                    Id = x.CategoryId,
                    Status = x.Status,
                    HyphenatedString = x.HyphenatedString,
                    MetaDescription = x.MetaDescription,
                    Name = x.Name,
                    MerchantCount = x.MerchantCount
                });
        }
    }
}
