using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class OfferModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string CouponCode { get; set; }

        public string EndDateTime { get; set; }

        public string Description { get; set; }

        public string HyphenatedString { get; set; }

        public bool IsFeatured { get; set; }

        public string Terms { get; set; }

        public int MerchantId { get; set; }

        public string MerchantLogoUrl { get; set; }

        public string OfferBackgroundImageUrl { get; set; }

        public string OfferBadge { get; set; }

        public bool IsCashbackIncreased { get; set; }

        public string WasRate { get; set; }

        public MerchantBasicModel Merchant { get; set; }

        public bool IsPremiumFeature { get; set; }
    }
}
