using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Category.Models
{
    public class AllCategoriesModel
    {
        public int TotalMerchants { get; set; }

        public IEnumerable<CategoryWithCountDTO> Categories { get; set; }
    }
}
