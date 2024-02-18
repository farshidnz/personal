using System;
using System.Threading;

namespace Cashrewards3API.Features.MemberClick
{
    public class BupaUniqueGeneratorService
        : IIdGeneratorService
    {
        protected static readonly Random RandomGenerator = new Random();

        private const int DIGIT_MIN = 1000;
        private const int DIGIT_MAX = 10000;
        private long _currentDigit;

        public BupaUniqueGeneratorService()
        {
            _currentDigit = DIGIT_MIN;
        }

        public string GetUniqueId(int memberId, int clientId)
        {
            var stamp = this.generateStamp();
            var digit = generateDigit();

            var generatedId = $"{stamp}{digit}";

            return $"b{generatedId}";
        }

        private string generateStamp()
        {
            var dateTime = DateTimeOffset.Now.UtcDateTime;

            return
                $"{dateTime.Year.ToString().Substring(2)}{dateTime.ToString("MM").Substring(1)}{dateTime.ToString("dd").Substring(1)}{dateTime.ToString("mmssfff")}{RandomGenerator.Next(1, 9).ToString()}";
        }

        private string generateDigit()
        {
            var number = Interlocked.Increment(ref _currentDigit);

            var digit = number % DIGIT_MAX;
            return digit.ToString("D4");
        }
    }
}
