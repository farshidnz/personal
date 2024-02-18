using Cashrewards3API.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantHyphenatedStringModel: IHavePremiumDisabled, IHaveIsPaused
    {
        public string HyphenatedString { get; set; }

        public bool? IsPremiumDisabled { get; set; }

        public int ClientId { get; set; }

        public decimal ClientCommission { get; set; }
        public bool IsPaused { get; set; }
    }
}
