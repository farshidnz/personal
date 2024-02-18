using System;

namespace Cashrewards3API.Common.Utils.Extensions
{
    public static class DecimalExtension
    {
        public static decimal RoundToTwoDecimalPlaces(this decimal d)
        {
            return Math.Round(d, 2);
        }
    }
}
