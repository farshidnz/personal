using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;

namespace Cashrewards3API.Features.Person.Model
{
    public class PersonModel
    {
        public int PersonId { get; set; }
        public Guid CognitoId { get; set; }

        public PremiumStatusEnum PremiumStatus { get; set; }

        public string OriginationSource { get; set; }

        public string MemberNewId { get; set; } 

        public int CashRewardsMemberId { get; set; }

        public virtual List<Member.Model.Member> Members { get; set; }
    }
}
