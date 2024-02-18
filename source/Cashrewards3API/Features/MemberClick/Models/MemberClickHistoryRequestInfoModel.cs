using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.MemberClick.Models
{
    public class MemberClickHistoryRequestInfoModel
    {
        public int MemberId { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
    }
}
