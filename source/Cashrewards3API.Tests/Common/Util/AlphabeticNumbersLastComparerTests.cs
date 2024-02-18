using Cashrewards3API.Common.Utils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Common.Util
{
    [TestFixture]
    public class AlphabeticNumbersLastComparerTests
    {

        [Test]
        public void AlphabeticNumbersLastComparer_ShouldOrderAlphabetically_WithItemsStartingWithANumberLast()
        {
            var items = new string[] { "10can", "frog", "344", "1ball", "apple", "advertise", "777", "grape" };

            var orderedItems = items.OrderBy(i => i, new AlphabeticNumbersLastComparer()).ToList();

            orderedItems[0].Should().Be("advertise");
            orderedItems[1].Should().Be("apple");
            orderedItems[2].Should().Be("frog");
            orderedItems[3].Should().Be("grape");
            orderedItems[4].Should().Be("10can");
            orderedItems[5].Should().Be("1ball");
            orderedItems[6].Should().Be("344");
            orderedItems[7].Should().Be("777");
        }
    }
}
