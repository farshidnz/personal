using Cashrewards3API.Common.Utils.Extensions;
using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantTier
    {
        public int MerchantTierId { get; set; }

        public int MerchantId { get; set; }

        public string TierDescription { get; set; }

        public decimal Commission { get; set; }

        public decimal ClientComm { get; set; }

        public decimal MemberComm { get; set; }

        public int TierCommTypeId { get; set; }

        public decimal ClientCommission => Math.Round(Commission * (ClientComm / 100) * (MemberComm / 100), 2);

        public string ClientCommissionString => MerchantTierSummaryBase.GetTierCommissionString(ClientCommission, TierCommTypeId);

        public DateTime EndDate { get; set; }

        public DateTime EndDateUtc { get; set; }

        public string TierImageUrl { get; set; }

        public string TierDescriptionLong { get; set; }

        public string TrackingLink { get; set; }

        public string TierSpecialTerms { get; set; }

        public string TierExclusions { get; set; }

        public List<MerchantTierLink> TierLinks { get; set; }

        public string TierReference { get; set; }

        public string TierName { get; set; }

        public int? CurrencyId { get; set; }

        public bool IsAdvancedTier { get; set; }

        public int Status { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime StartDateUtc { get; set; }

        public int TierTypeId { get; set; }

        public List<int> ClientIds { get; set; } = new List<int>();

        public int Ranking { get; set; }

        public bool IsFeatured { get; set; }

        public string BadgeCode { get; set; }

        public PremiumTier Premium { get; set; }

    }

}