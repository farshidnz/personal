#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class DateRule
    {
        public DateTime? Before { get; set; }
        public DateTime? After { get; set; }
        public bool Required { get; set; }
    }

    public class FirstPurchaseWindow
    {
        public int? Max { get; set; }
        public int? Min { get; set; }
        public bool Required { get; set; }
    }

    public class SaleValue
    {
        public decimal? Max { get; set; }
        public decimal? Min { get; set; }
        public bool Required { get; set; }
    }

    public class MerchantIdRuleModel
    {
        public string? In { get; set; }
        public string? Not_In { get; set; }
        public bool Required { get; set; }
    }

    public class StoreTypeRuleModel
    {
        public string? In { get; set; }
        public string? Not_In { get; set; }
        public bool Required { get; set; }
    
    }

    public class CategoryIdRuleModel
    {
        public string? In { get; set; }
        public string? Not_In { get; set; }
        public bool Required { get; set; }
    
    }

    public class NetworkIdRuleModel
    {
        public string? In { get; set; }
        public string? Not_In { get; set; }
        public bool Required { get; set; }

    }

    public class NewTransactionRulesModel
    {
        public DateRule? Member_Joined_Date { get; set; }
        public FirstPurchaseWindow? First_Purchase_Window { get; set; }
        public SaleValue? Sale_Value { get; set; }
        public DateRule? Sale_Date { get; set; }
        public MerchantIdRuleModel? Merchant_Id { get; set; }
        public StoreTypeRuleModel? Store_Type { get; set; }
        public CategoryIdRuleModel? Category_Id { get; set; }
        public NetworkIdRuleModel? Network_Id { get; set; }
    }

    public class QualifyingTransactionsRequestModel
    {
        public int MemberId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool HasGst { get; set; }
        public NewTransactionRulesModel? New_Transaction { get; set; }

    }
}


