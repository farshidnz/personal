using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Merchant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Repository
{
    public interface ITrendingMerchantRepository
    {
        Task<IEnumerable<MerchantViewModel>> GetMerchantsByIdListAsync(IList<int> clientIds, IEnumerable<int> merchantIds, bool isMobileApp = false);

        Task<IEnumerable<int>> GetMerchantIdsByCategoryAsync(IList<int> clientIds, int categoryId);
    }

    public class TrendingMerchantRepository : ITrendingMerchantRepository
    {
        private readonly IReadOnlyRepository _readOnlyRepository;

        public TrendingMerchantRepository(IReadOnlyRepository readOnlyRepository)
        {
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<IEnumerable<MerchantViewModel>> GetMerchantsByIdListAsync(
            IList<int> clientIds, IEnumerable<int> merchantIds, bool isMobileApp = false)
        {
            const string SQLQuery = @"SELECT * FROM MaterialisedMerchantView
                                      WHERE ClientId in @ClientIds";

            var merchants = await _readOnlyRepository.QueryAsync<MerchantViewModel>(SQLQuery, new
            {
                ClientIds = clientIds
            });

            return merchants
                .Where(merchant => merchantIds.Contains(merchant.MerchantId) &&
                            (!isMobileApp ||
                            (isMobileApp && (!merchant.IsMobileAppEnabled.HasValue ||
                                              merchant.IsMobileAppEnabled == true))))
                .OrderBy(merchant => merchant.MerchantName)
                .ToList();
        }

        public async Task<IEnumerable<int>> GetMerchantIdsByCategoryAsync(IList<int> clientIds, int categoryId)
        {
            const string SQLQuery = @"SELECT merchant.MerchantId AS MerchantId FROM MaterialisedMerchantFullView merchant 
                                      INNER JOIN MerchantCategoryMap merchantCategory  
                                      ON merchant.MerchantId = merchantCategory.MerchantId
                                      WHERE merchant.ClientId IN @ClientIds AND merchantCategory.CategoryId = @CategoryId";

            var merchantIds = await _readOnlyRepository.QueryAsync<MerchantByCategorId>(SQLQuery, new
            {
                ClientIds = clientIds,
                CategoryId = categoryId
            });

            return merchantIds.Select(m => m.MerchantId).ToList();
        }
    }
}
