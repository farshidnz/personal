using System;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class DeclineBonusTransaction
    {
        private DeclineBonusTransaction()
        {
        }

        public int TransactionId { get; private set; }
        public int TransactionStatusId { get; private set; }
        public int NetworkTranStatusId { get; private set; }
        public DateTime? LastUpdated { get; private set; }

        public static DeclineBonusTransaction Create(int transactionId)
            => new DeclineBonusTransaction
            {
                TransactionStatusId = (int) TransactionStatusEnum.Declined,
                NetworkTranStatusId = (int) TransactionStatusEnum.Declined,
                LastUpdated = DateTime.Now,
                TransactionId = transactionId
            };
    }
}