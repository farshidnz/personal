using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.MemberClick.Models
{
    public class MemberClickHistoryModel
    {
        public int ClickId { get; set; }
        public int MerchantId { get; set; }
        public int ClickCount { get; set; }
        public DateTime DateCreated { get; set; }
        public string MerchantName { get; set; }
        public string HyphenatedString { get; set; }
        public int MemberId { get; set; }
        public bool AdBlockEnabled { get; set; }
        public int NetworkId { get; set; }
        public DateTime DateCreatedUtc { get; set; }
        public int TotalCount { get; set; }
    }
}
