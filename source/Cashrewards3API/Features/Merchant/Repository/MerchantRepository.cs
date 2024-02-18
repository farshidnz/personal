using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Repository
{
    public class MerchantRepository : IMerchantRepository
    {
        private readonly IReadOnlyRepository _readOnlyRepository;

        public MerchantRepository(IReadOnlyRepository readOnlyRepository)
        {
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<IEnumerable<MerchantFullView>> GetMerchantsByClientIdsAndHyphenatedStringAsync(IEnumerable<int> clientId, string hyphenatedId)
        {
            var queryString = @"SELECT * FROM MaterialisedMerchantFullView 
                                WHERE ClientId IN @ClientIds AND Hyphenatedstring = @Hyphenatedstring";

            var merchantList = await _readOnlyRepository.Query<MerchantFullView>(queryString, new
            {
                ClientIds = clientId,
                Hyphenatedstring = hyphenatedId
            });

            return merchantList;
        }

        public async Task<MerchantFullView> GetMerchantByClientIdAsync(int clientId, int merchantId)
        {
            var queryString = @"SELECT * FROM MaterialisedMerchantFullView 
                             WHERE ClientId = @ClientId AND MerchantId = @MerchantId";

            var merchants = await _readOnlyRepository.Query<MerchantFullView>(queryString, new
            {
                ClientId = clientId,
                MerchantId = merchantId
            });

            return merchants.FirstOrDefault();
        }

        public async Task<IEnumerable<StoreModel>> GetMerchantStores(int merchantId)
        {
            var queryString = @"SELECT S.* FROM Store S 
                                INNER JOIN MerchantStore MS on S.StoreId = MS.StoreId
                                WHERE Status = 1 AND MerchantId = @MerchantId";

            var merchantStores = await _readOnlyRepository.Query<StoreModel>(queryString, new
            {
                MerchantId = merchantId
            });

            return merchantStores;
        }

        public async Task<IEnumerable<OfferViewModel>> GetMerchantOfferViewsAsync(IEnumerable<int> clientIds, int merchantId)
        {
            string SQLQuery = @"SELECT * FROM MaterialisedOfferView  
                                WHERE ClientId IN @ClientIds AND MerchantId = @MerchantId";

            var offerViews = await _readOnlyRepository.Query<OfferViewModel>(SQLQuery,
                    new
                    {
                        ClientIds = clientIds,
                        MerchantId = merchantId
                    });

            return offerViews;
        }

        public async Task<IEnumerable<MerchantTierView>> GetMerchantTierViewsAsync(int clientId, int merchantId)
        {
            var queryString = @"SELECT * FROM MaterialisedMerchantTierView 
                             WHERE ClientId = @ClientId AND MerchantId = @MerchantId";

            var tiers = await _readOnlyRepository.Query<MerchantTierView>(queryString, new
            {
                ClientId = clientId,
                MerchantId = merchantId
            });

            return tiers;
        }

        public async Task<IEnumerable<MerchantTierViewWithBadge>> GetMerchantTierViewsWithBadgeAsync(IEnumerable<int> clientIds, int merchantId)
        {
            var queryString = @"SELECT t.*, b.BadgeCode
                                FROM MaterialisedMerchantTierView t
                                LEFT JOIN EntityBadgeMapping e ON e.EntityId = t.MerchantTierId
                                LEFT JOIN Badge b ON e.BadgeId = b.BadgeId
                                WHERE t.ClientId IN @ClientIds AND t.MerchantId = @MerchantId
                                AND (e.Status = 1 OR e.Status IS NULL)";

            var tiers = await _readOnlyRepository.Query<MerchantTierViewWithBadge>(queryString, new
            {
                ClientIds = clientIds,
                MerchantId = merchantId
            });

            return tiers;
        }

        public async Task<IEnumerable<MerchantTierLinkModel>> GetMerchantTierLinks(int merchantTierId)
        {
            var queryString = @"SELECT * FROM MerchantTierLink 
                             WHERE MerchantTierId = @MerchantTierId";

            var merchantTierLinks = await _readOnlyRepository.Query<MerchantTierLinkModel>(queryString, new
            {
                MerchantTierId = merchantTierId
            });

            return merchantTierLinks;
        }

        public async Task<MerchantTier> GetMerchantTierModel(int merchantTierId)
        {
            var queryString = @"SELECT * FROM MerchantTier 
                             WHERE MerchantTierId = @MerchantTierId";

            var merchantTiers = await _readOnlyRepository.Query<MerchantTier>(queryString, new
            {
                MerchantTierId = merchantTierId
            });

            return merchantTiers.FirstOrDefault();
        }

    }
}
