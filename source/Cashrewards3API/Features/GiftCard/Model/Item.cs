using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.GiftCard.Model
{
    public class Item
    {
        public int ItemId { get; set; }

        public string BackgroundUrl { get; set; }

        public string ItemBadge { get; set; }

        public int ItemType { get; set; }

        public string ProductName { get; set; }
    }
}
