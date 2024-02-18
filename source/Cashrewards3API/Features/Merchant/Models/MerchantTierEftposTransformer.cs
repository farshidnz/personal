using Cashrewards3API.Common.Utils.Extensions;
using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantTierEftposTransformer
    {
        public int MerchantTierId { get; set; }
        public int MerchantId { get; set; }
        public decimal Commission { get; set; }
        public int TierCommTypeId { get; set; }
        public string TierSpecialTerms { get; set; }
        public string TierReference { get; set; }
        public int? CurrencyId { get; set; }
        public string NetworkId { get; set; }
        public string GstStatusId { get; set; }
        public string ApprovalWaitDays { get; set; }
        public string NetworkKey { get; set; }
    }
}