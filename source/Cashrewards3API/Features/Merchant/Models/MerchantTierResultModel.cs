using System;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantTierResultModel
    {
        protected bool Equals(MerchantTierResultModel other)
        {
            return Id == other.Id && Name == other.Name && Commission == other.Commission &&
                   CommissionType == other.CommissionType && EndDateTime == other.EndDateTime && Terms == other.Terms &&
                   Exclusions == other.Exclusions;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MerchantTierResultModel) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Commission, CommissionType, EndDateTime, Terms, Exclusions);
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Commission { get; set; }

        public string CommissionType { get; set; }

        public string CommissionString { get; set; }

        public string EndDateTime { get; set; }

        public string Terms { get; set; }

        public string Exclusions { get; set; }

        public MerchantPremiumTierResultModel Premium { get; set; }
    }
   
    public class MerchantPremiumTierResultModel
    {
        public decimal Commission { get; set; }

        public string CommissionType { get; set; }

        public string CommissionString { get; set; }
    }

}