using System.ComponentModel;
using System.Runtime.Serialization;

namespace Cashrewards3API.Enum
{
    public enum TransactionStatusEnum
    {
            [Description("Pending")]
            [EnumMember]
            Pending = 100,
            [Description("Approved")]
            [EnumMember]
            Approved = 101,
            [Description("Declined")]
            [EnumMember]
            Declined = 102

    }
}
