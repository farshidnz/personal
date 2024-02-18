using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Helpers
{
    public class StepDefinitions
    {
        public static bool IsGreaterThanZeroString(string value) =>
            value.Equals(">0", StringComparison.InvariantCultureIgnoreCase);

        public static bool IsEmptyString(string value) =>
         String.IsNullOrWhiteSpace(value) || value.Equals("empty", StringComparison.InvariantCultureIgnoreCase);

        public static bool IsTrue(string value) =>
            !String.IsNullOrWhiteSpace(value) && (
            value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
            value.Equals("on", StringComparison.InvariantCultureIgnoreCase) ||
            value.Equals("yes", StringComparison.InvariantCultureIgnoreCase)
                );

    }
}
