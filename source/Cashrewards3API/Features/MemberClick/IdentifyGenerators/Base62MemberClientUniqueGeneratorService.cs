using System;
using System.Numerics;

namespace Cashrewards3API.Features.MemberClick
{
    public class Base62MemberClientUniqueGeneratorService
        : AbstractBase62Generator, IIdGeneratorService
    {
        public string GetUniqueId(int memberId, int clientId)
        {
            var stamp = this.generateStamp();
            var digit = generateDigit();

            var number = $"{digit}{memberId}{clientId}{stamp}";

            var generatedId = computeHash(BigInteger.Parse(number));
            var uniqueInitials = (clientId == (int)Common.Constants.Clients.MoneyMe) ? "mm" : "cg";

            return $"{uniqueInitials}{generatedId}";
        }

        protected override string generateStamp()
        {
            return $"{DateTimeOffset.Now.Ticks.ToString()}{RandomGenerator.Next(100000, 999999)}";
        }
    }
}
