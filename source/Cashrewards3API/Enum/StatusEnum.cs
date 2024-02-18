using System.ComponentModel;
using System.Runtime.Serialization;

namespace Cashrewards3API.Enum
{
    public enum StatusEnum
    {
        [Description("Deleted")]
        [EnumMember]
        Deleted = 0,

        [Description("Active")]
        [EnumMember]
        Active = 1,

        [Description("InActive")]
        [EnumMember]
        InActive = 2,

        [Description("")]
        [EnumMember]
        NotAssigned = 100
    }
}