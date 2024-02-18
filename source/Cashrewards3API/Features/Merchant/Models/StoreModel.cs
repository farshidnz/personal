using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class StoreModel
    {
        public int StoreId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string PostCode { get; set; }
        public Nullable<int> StateId { get; set; }
        public Nullable<decimal> Latitudes { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public Nullable<int> StoreTypeId { get; set; }
        public string MetaData { get; set; }

    }

}
