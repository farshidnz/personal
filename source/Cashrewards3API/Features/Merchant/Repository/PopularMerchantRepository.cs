using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Merchant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Repository
{
    public interface IPopularMerchantRepository
    {
        Task<IEnumerable<MerchantViewModel>> GetPopularMerchantsFromDbAsync(int clientId, IEnumerable<int> popularMerchantIds);
    }

    public class PopularMerchantRepository : IPopularMerchantRepository
    {
        private readonly IReadOnlyRepository _readOnlyRepository;

        public PopularMerchantRepository(IReadOnlyRepository readOnlyRepository)
        {
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<IEnumerable<MerchantViewModel>> GetPopularMerchantsFromDbAsync(int clientId, IEnumerable<int> popularMerchantIds)
        {
            const string sqlQuery = @"SELECT * FROM MaterialisedMerchantView  
                                      WHERE ClientId = @ClientId AND IsPopular = 1 AND MerchantId in @MerchantIds";

            return await _readOnlyRepository.QueryAsync<MerchantViewModel>(sqlQuery,
                new
                {
                    ClientId = clientId,
                    MerchantIds = popularMerchantIds.ToArray()
                });
        }
    }
}
