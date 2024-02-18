using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.GiftCard.Model
{
    public class GiftCard
    {
        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        public string BannerImageUrl { get; set; }

        public string BannerImageTabUrl { get; set; }

        public string BannerImageMobileUrl { get; set; }

        public string MidbannerImageUrl { get; set; }

        public string MidbannerImageTabUrl { get; set; }

        public string MidbannerImageMobileUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string LongDescription { get; set; }

        public List<Category> Categories { get; set; }

        public string BottomBannerImageUrl { get; set; }

        public string BottomBannerImageTabUrl { get; set; }

        public string BottombannerImageMobileUrl { get; set; }
    }
}
