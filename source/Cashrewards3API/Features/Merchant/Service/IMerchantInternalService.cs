using Cashrewards3API.Features.Merchant.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant
{
    public interface IMerchantInternalService
    {
        Task<MerchantTier> CreateInternalMerchantTier(
          CreateInternalMerchantTierRequestModel requestModel);

        Task UpdateInternalMerchantTier(UpdateInternalMerchantTierRequestModel requestModel);

        Task DeactivateMerchantTier(int merchantTierId);

        Task<Boolean> ExistsMerchantTierClient(int merchantTierId, int clientId, DateTime dateTimeUtc);

        Task<IList<MerchantTierEftposTransformer>> GetActiveMerchantTiers(int merchantId, DateTime dateTimeUtc, int? top);

    }
}