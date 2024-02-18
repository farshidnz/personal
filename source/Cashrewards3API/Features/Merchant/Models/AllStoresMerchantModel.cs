namespace Cashrewards3API.Features.Merchant.Models
{
    public class AllStoresMerchantModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string HyphenatedString { get; set; }

        public string LogoUrl { get; set; }

        public decimal ClientCommission { get; set; }

        public string CommissionString { get; set; }

        public bool Online { get; set; }

        public bool InStore { get; set; }

        public PremiumDto Premium { get; set; }
    }

    public class PremiumDto
    {
        public decimal Commission { get; set; }
        public bool IsFlatRate { get; set; }
        public string ClientCommissionString { get; set; }
    }
}