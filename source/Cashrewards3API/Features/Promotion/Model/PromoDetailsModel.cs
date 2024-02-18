using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Promotion.Model
{
    public class PromoDetailsModel
    {
        public string code { get; set; }

        public string status { get; set; }

        public bool valid { get; set; }

        public string terms_and_condition { get; set; }

        public string reason { get; set; }

        public Promotion promotion { get; set; }

        public class Promotion
        {
            public string bonus_value { get; set; }

            public string bonus_type { get; set; }

            public Rules rules { get; set; }
        }

        public class Rules
        {
            public Coupon coupon { get; set; }

            public MemberJoinedDetails member_joined_date { get; set; }

            public PurchaseRules first_purchase_window { get; set; }

            public SaleRule sale_value { get; set; }

            public List<string> gst { get; set; }

            public AdditionalRule merchant_id { get; set; }

            public AdditionalRule store_type { get; set; }

            public AdditionalRule category_id { get; set; }

            public List<string> first_purchase { get; set; }
        }

        public class SaleRule
        {
            public string min { get; set; }

            public bool required { get; set; }
        }

        public class Coupon
        {
            public string equals { get; set; }

            public bool required { get; set; }
        }

        public class MemberJoinedDetails
        {
            public DateTime after { get; set; }

            public bool required { get; set; }

            public DateTime before { get; set; }
        }

        public class PurchaseRules
        {
            public string max { get; set; }

            public bool required { get; set; }
        }

        public class AdditionalRule
        {
            public bool required { get; set; }

            public string @in { get; set; }

            public string not_in { get; set; }
        }
    }

    
}
