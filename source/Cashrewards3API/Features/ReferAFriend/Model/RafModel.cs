using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Features.ReferAFriend.Model
{
    public class RafModel
    {
        public string AccessCode { get; set; }
        public decimal? BonusValue { get; set; }
        public int? QualifiedTransactionId { get; set; }
        public int? FriendBuyConversionId { get; set; }
        public TransactionStatusEnum TransactionStatusId { get; set; }
        public TransactionTypeEnum TransactionTypeId { get; set; }
    }
}
