using Cashrewards3API.Enum;
using System;

namespace Cashrewards3API.Common.Services.Model
{
    public class PersonPremiumStatusHistory
    {
        public int? PersonId { get; set; }

        public int ClientId { get; set; }
        public DateTime StartedAtUTC { get; set; }
        public DateTime EndedAtUTC { get; set; }
        public PremiumStatusEnum PremiumStatus { get; set; }
        public DateTime CreatedDateUTC { get; set; }
        public DateTime? UpdatedDateUTC { get; set; }
    }
}