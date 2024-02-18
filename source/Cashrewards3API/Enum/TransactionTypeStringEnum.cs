using System.ComponentModel;

namespace Cashrewards3API.Enum
{
    public enum TransactionTypeStringEnum
    {
        [Description("Cashback")]
        Discount = 0,
        [Description("Savings")]
        Savings = 1,
        [Description("Payment")]
        Payment = 2
    }
}
