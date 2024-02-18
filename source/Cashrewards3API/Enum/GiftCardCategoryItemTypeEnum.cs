using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum GiftCardCategoryItemTypeEnum
    {
        Merchant = 1,
        Offer = 2,
        Offer2Go = 3,  // Offer click direct to Go page
        GiftCard2Go = 4, //Giftcard click direct to Go page.
        GiftCardWidget = 5 //Giftcard click open the truerewards widget.
    }
}
