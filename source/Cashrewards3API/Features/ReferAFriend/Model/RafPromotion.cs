using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ReferAFriend.Model
{
    public class RafPromotion
    {
        public decimal BonusValue { get; set; }
        public string BonusType { get; set; }
        public string TransactionStatus { get; set; }
        public string TransactionType { get; set; }
        public int TransactionTypeId { get; set; }
        public RafRules Rules { get; set; }

    }
}
