using Cashrewards3API.Enum;
using Cashrewards3API.Features.Member.Model;
using System;
using System.Collections.Generic;

namespace Cashrewards3API.Common.Services.Model
{
    public class Person
    {
        public int? PersonId { get; set; }
        public Guid CognitoId { get; set; }

        public string OriginationSource { get; set; }
        public PremiumStatusEnum PremiumStatus { get; set; }
        public DateTime CreatedDateUTC { get; set; }
        public DateTime? UpdatedDateUTC { get; set; }

        public virtual List<Member> Members { get; set; }
    }
}