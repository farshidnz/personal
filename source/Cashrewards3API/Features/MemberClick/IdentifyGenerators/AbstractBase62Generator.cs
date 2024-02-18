using System;
using System.Linq;
using System.Numerics;

namespace Cashrewards3API.Features.MemberClick
{
    public abstract class AbstractBase62Generator
    {
        private const int DIGIT_MIX = 11;
        private const int DIGIT_MAX = 99;

        protected static readonly Random RandomGenerator = new Random();

        private static readonly char[] Alpha = new char[]
        {
           '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
        'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
        'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
        'e', 'f', 'g', 'g', 'i', 'j', 'k', 'l', 'm', 'n',
        'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
        'y', 'z'
        };

        protected string computeHash(BigInteger number)
        {
            var baseNum = new BigInteger(Alpha.Count());

            var hash = string.Empty;

            while (number >= baseNum)
            {
                BigInteger index;
                number = BigInteger.DivRem(number, baseNum, out index);
                hash = Alpha[(int)index] + hash;
            }

            hash = Alpha[(int)number] + hash;

            return hash;
        }

        protected string generateDigit()
        {
            return RandomGenerator.Next(DIGIT_MIX, DIGIT_MAX).ToString();
        }

        protected abstract string generateStamp();
    }
}
