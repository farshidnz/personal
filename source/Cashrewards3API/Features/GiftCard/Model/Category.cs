using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.GiftCard.Model
{
    public class Category
    {
        public string CategoryTitle { get; set; }

        public string CategoryType { get; set; }

        public List<Item> Items { get; set; }
    }
}
