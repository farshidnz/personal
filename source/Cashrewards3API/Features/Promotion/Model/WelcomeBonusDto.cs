using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Promotion.Model
{
    public class MemberBonusDto
    {
        public decimal Bonus { get; set; }

        public decimal MinSpend { get; set; }

        public int PurchaseWindow { get; set; }

        public string TermsAndConditions { get; set; }

        public bool ExcludeGST { get; set; }

        public MemberBonusRule Merchant { get; set; }

        public MemberBonusRule StoreType { get; set; }

        public MemberBonusRule Category { get; set; }
    }

    public class MemberBonusRule
    {
        public IEnumerable<string> In { get; set; }

        public IEnumerable<string> NotIn { get; set; }
    }
}
