using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Utils
{
    public class AlphabeticNumbersLastComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if(StartsWithNumber(x) && !StartsWithNumber(y))
            {
                return 1;
            }
            if (!StartsWithNumber(x) && StartsWithNumber(y))
            {
                return -1;
            }

            return x.CompareTo(y);

        }

        private bool StartsWithNumber(string s)
        {
            return Char.IsNumber(s.FirstOrDefault());
        }


    }
}
