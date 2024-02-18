using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Transaction.Model
{
    public class MemberTransactionRequestInfoModel
    {
        public int MemberId { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
    }
}
