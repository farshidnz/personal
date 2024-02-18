namespace Cashrewards3API.Features.Merchant.Models
{
    public class UpdateInternalMerchantTierRequestModel : CreateInternalMerchantTierRequestModel
    {
        public int MerchantTierId { get; set; }
    }
}