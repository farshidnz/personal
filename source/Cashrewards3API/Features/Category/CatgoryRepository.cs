using Cashrewards3API.Common.Services;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Category.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Category
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IReadOnlyRepository _readOnlyRepository;

        public CategoryRepository(IReadOnlyRepository readOnlyRepository)
        {
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoriesByClientIdAndStatusAsync(List<int> clientIds, Status status)
        {
            var SQLQuery = @"SELECT * FROM Category cat 
                             INNER JOIN ClientCategoryMap map ON cat.CategoryId = map.CategoryId
                             WHERE map.ClientId IN @ClientId AND Status = @CategoryStatus 
                             ORDER BY  cat.Ranking DESC";

            return (await _readOnlyRepository.QueryAsync<CategoryModel>(SQLQuery,
                    new
                    {
                        ClientId = clientIds,
                        CategoryStatus = (int)status
                    })
                );
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoriesByClientIdAsync(List<int> clientId)
        {
            var SQLQuery = @"SELECT * FROM Category cat 
                             INNER JOIN ClientCategoryMap map ON cat.CategoryId = map.CategoryId
                             WHERE map.ClientId IN @ClientId 
                             ORDER BY  cat.Ranking DESC";

            return (await _readOnlyRepository.QueryAsync<CategoryModel>(SQLQuery,
                    new
                    {
                        ClientId = clientId
                    })
                );
        }

        public async Task<IEnumerable<CategoryModel>> GetRootCategoriesByClientIdAndStatusWithCountsAsync(List<int> clientIds, Status status)
        {
            var SQLQuery = @"SELECT 
                                c.CategoryId,
                                c.HyphenatedString,
                                c.Name, 
                                c.Status, 
                                c.RootCategoryId, 
                                c.DisplayName, 
                                c.Ranking,
                                c.MetaDescription,
                                COUNT(mcm.CategoryId) as MerchantCount
                            FROM Category c
                            INNER JOIN MerchantCategoryMap mcm ON c.CategoryId = mcm.CategoryId
                            INNER JOIN (
	                            SELECT HyphenatedString, Max(MerchantId) as MerchantId
	                            FROM MaterialisedMerchantFullView 
	                            WHERE ClientId IN @ClientId
	                            GROUP BY HyphenatedString
                            ) as m ON m.MerchantId = mcm.MerchantId
                            WHERE c.RootCategoryId IS NULL
                            AND c.Status = @CategoryStatus
                            GROUP BY c.CategoryId, c.HyphenatedString, c.Name, c.Status, c.RootCategoryId, c.DisplayName, c.Ranking, c.Metadescription";

            return (await _readOnlyRepository.QueryAsync<CategoryModel>(SQLQuery,
                    new
                    {
                        CategoryStatus = (int)status,
                        ClientId = clientIds
                    })
                );
        }

        public async Task<IEnumerable<CategoryModel>> GetNonRootCategoriesAsync(Status status, CategoryTypeEnum categoryType)
        {
            var query = @"SELECT * FROM Category
                        WHERE Status = @Status AND RootCategoryId IS NULL AND CategoryTypeId = @CategoryTypeId
                        ORDER BY Name";

            return await _readOnlyRepository.QueryAsync<CategoryModel>(query,
                new
                {
                    Status = (int)status,
                    CategoryTypeId = (int)categoryType
                });
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoriesByMerchantIdAsync(int merchantId)
        {
            var SQLQuery = @"SELECT * FROM Category cat 
                             INNER JOIN MerchantCategoryMap mcm ON cat.RootCategoryId = mcm.CategoryId
                             WHERE mcm.MerchantId = @merchantId 
                             ORDER BY  cat.Ranking DESC";

            return (await _readOnlyRepository.QueryAsync<CategoryModel>(SQLQuery,
                    new
                    {
                        merchantId
                    })
                );
        }
    }
}
