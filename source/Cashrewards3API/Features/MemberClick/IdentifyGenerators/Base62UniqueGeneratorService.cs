using System;
using System.Numerics;
using Serilog.Core;
using Cashrewards3API.Common;

namespace Cashrewards3API.Features.MemberClick
{
    public class Base62UniqueGeneratorService
           : AbstractBase62Generator, IIdGeneratorService
    {
        public string GetUniqueId(int memberId, int clientId)
        {
            var stamp = this.generateStamp();
            var digit = generateDigit();

            var uniqueNumber = $"{digit}{stamp}";

            var generatedId = computeHash(BigInteger.Parse(uniqueNumber));
            var uniqueInitials = (clientId == (int) Common.Constants.Clients.MoneyMe) ? "mm" : "cg";

            return $"{uniqueInitials}{generatedId}";
        }

        protected override string generateStamp()
        {
            return $"{DateTimeOffset.Now.Ticks.ToString()}{RandomGenerator.Next(1000, 9999)}{RandomGenerator.Next(1000, 9999)}";
        }
    }
}
