using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.GiftCard.Model
{
    public class GiftCardDto
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

        public List<CategoryDto> Categories { get; set; }

        public string BottomBannerImageUrl { get; set; }

        public string BottombannerImageTabUrl { get; set; }

        public string bottomBannerImageMobileUrl { get; set; }


        public class CategoryDto
        {
            public string CategoryTitle { get; set; }

            public string CategoryType { get; set; }

            public List<ItemDto> Items { get; set; }

            public class ItemDto
            {
                public int ItemId { get; set; }

                public string Name { get; set; }

                public string HyphenatedString { get; set; }

                public string RateString { get; set; }

                public string MerchantHyphenatedString { get; set; }

                public string BackgroundUrl { get; set; }

                public string ItemBadge { get; set; }

                public int ItemType { get; set; }

                public string ProductName { get; set; }

                public PremiumDto Premium { get; set; }

                public class PremiumDto
                {
                    public string RateString { get; set; }
                }
            }

        }
    }
}
