using Cashrewards3API.Common.Model;
using Cashrewards3API.Features.Merchant.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.CardLinkedMerchant
{  
    public class CardLinkedMerchant
    {
        public int MerchantId { get; set; }
    }

    public class CardLinkedMerchantFullViewModel
    {
        [Key]
        public int MerchantId { get; set; }
        public int ClientId { get; set; }
        public bool IsMobileAppEnabled { get; set; }
        public string MerchantBadgeCode { get; set; }
    }

    public partial class CardLinkedMerchantViewModel: IHavePremiumDisabled
    {
        public string MerchantName { get; set; }
        public string MerchantHyphenatedString { get; set; }
        public string CardIssuer { get; set; }
        public string OfferId { get; set; }
        public System.DateTime EndDate { get; set; }
        public string CardLinkedSpecialTerms { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string LogoImageUrl { get; set; }
        public bool InStore { get; set; }
        public Nullable<bool> IsFeatured { get; set; }
        public Nullable<bool> IsHomePageFeatured { get; set; }
        public Nullable<bool> IsPopular { get; set; }
        public Nullable<decimal> Commission { get; set; }
        public Nullable<decimal> ClientComm { get; set; }
        public int TierCommTypeId { get; set; }
        public int MerchantId { get; set; }
        public int ClientId { get; set; }
        public bool IsFlatRate { get; set; }
        public bool? IsPremiumDisabled { get; set; }
    }

    public class CardLinkedMerchantDto
    {
        public CardLinkedMerchantDto()
        {
            Channels = new List<string>();
        }
        public string MerchantName { get; set; }

        public string MerchantHyphenatedString { get; set; }

        public string CardIssuer { get; set; }

        public string OfferId { get; set; }

        public DateTime EndDate { get; set; }

        public string CardLinkedSpecialTerms { get; set; }

        public string BackgroundImageUrl { get; set; }

        public string BannerImageUrl { get; set; }

        public string LogoImageUrl { get; set; }

        public List<string> Channels { get; set; }

        public string CommissionString { get; set; }

        public int MerchantId { get; set; }

        public int ClientId { get; set; }

        public bool? IsFeatured { get; set; }

        public bool? IsHomePageFeatured { get; set; }

        public bool? IsPopular { get; set; }

        public string MerchantBadge { get; set; }

        public Nullable<decimal> Commission { get; set; }

        public bool IsFlatRate { get; set; }

        public string CommissionType { get; set; }

        public PremiumCardLinkedMerchant Premium { get; set; }
    }

    public class PremiumCardLinkedMerchant
    {
        public string CommissionString { get; set; }
        public Nullable<decimal> Commission { get; set; }
        public bool IsFlatRate { get; set; }
        public string CommissionType { get; set; }
    }
}
