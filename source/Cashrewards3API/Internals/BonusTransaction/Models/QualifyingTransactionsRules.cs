using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class QualifyingTransactionsRules
    {
        public int MemberId { get; set; }
        public IEnumerable<int> AssoicatedMemberIds { get; set; }
        public decimal SaleValueMin { get; set; }
        public decimal SaleValueMax { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? NetworkId_In { get; set; }
        public string? NetworkId_Not_In { get; set; }


        public static QualifyingTransactionsRules Create(QualifyingTransactionsRequestModel requestModel)
        {
            var saleValue = requestModel?.New_Transaction?.Sale_Value;
            return new QualifyingTransactionsRules()
            {
                MemberId = requestModel.MemberId,
                SaleValueMin = saleValue?.Min ?? 0,
                SaleValueMax = saleValue?.Max ?? 0,
                StartDate = requestModel.StartDate,
                EndDate = requestModel.EndDate,
                NetworkId_In = requestModel.New_Transaction?.Network_Id?.In ?? null,
                NetworkId_Not_In = requestModel.New_Transaction?.Network_Id?.Not_In ?? null,
            };
        }
        
        
    }
}
