using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Repository
{
    public interface IMerchantRepository
    {
        Task<IEnumerable<MerchantFullView>> GetMerchantsByClientIdsAndHyphenatedStringAsync(IEnumerable<int> clientId, string hyphenatedId);
        Task<MerchantFullView> GetMerchantByClientIdAsync(int clientId, int merchantId);
        Task<IEnumerable<StoreModel>> GetMerchantStores(int merchantId);
        Task<IEnumerable<OfferViewModel>> GetMerchantOfferViewsAsync(IEnumerable<int> clientIds, int merchantId);
        Task<MerchantTier> GetMerchantTierModel(int merchantTierId);
        Task<IEnumerable<MerchantTierLinkModel>> GetMerchantTierLinks(int merchantTierId);
        Task<IEnumerable<MerchantTierView>> GetMerchantTierViewsAsync(int clientId, int merchantId);
        Task<IEnumerable<MerchantTierViewWithBadge>> GetMerchantTierViewsWithBadgeAsync(IEnumerable<int> clientIds, int merchantId);
    }
}