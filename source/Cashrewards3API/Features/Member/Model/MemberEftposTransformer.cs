using Cashrewards3API.Common.Utils.Extensions;
using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;

namespace Cashrewards3API.Features.Member.Models
{
    public class MemberEftposTransformer
    {
        public int MemberId { get; set; }
        public int ClientId { get; set; }
        public int Status { get; set; }
        public int? PersonId { get; set; }
        public Guid? CognitoId { get; set; }
        public int? PremiumStatus { get; set; }
    }
}