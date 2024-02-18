using System;

namespace Cashrewards3API.Common.Dto
{
    public static class Extensions
    {
        public static decimal RoundToTwoDecimalPlaces(this decimal d)
        {
            return Math.Round(d, 2);
        }
    }
}
