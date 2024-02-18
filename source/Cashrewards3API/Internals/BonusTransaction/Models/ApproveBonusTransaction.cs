using System;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class ApproveBonusTransaction
    {
        private ApproveBonusTransaction()
        {
        }
        
        public int TransactionId { get; private set; }
        public int TransactionStatusId { get; private set; }
        public int NetworkTranStatusId { get; private set; }
        public int TransactionStatusDeclined { get; private set; }
        public DateTime? LastUpdated { get; private set; }
        public DateTime? DateApproved { get; private set; }

        public static ApproveBonusTransaction Create(int transactionId)
            => new ApproveBonusTransaction
            {
                TransactionStatusId = (int)TransactionStatusEnum.Approved,
                NetworkTranStatusId = (int)TransactionStatusEnum.Approved,
                TransactionStatusDeclined = (int)TransactionStatusEnum.Declined,
                DateApproved = DateTime.Now,
                LastUpdated = DateTime.Now,
                TransactionId = transactionId
            };

    }
}
