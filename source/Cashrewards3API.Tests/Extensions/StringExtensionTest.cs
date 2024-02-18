using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Enum;
using Cashrewards3API.Extensions;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Mapper;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cashrewards3API.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionTest
    {
        [Test]
        [TestCase("9", 9)]
        [TestCase("95", 95)]
        [TestCase("ninty", null)]
        [TestCase("ninty", 0, 0)]
        [TestCase("9ninty", null)]
        [TestCase("9ninty", 0, 0)]
        [TestCase("9ninty", -1, -1)]
        [TestCase("9ninty", 9, 9)]
        [TestCase(" 9 ", 9)]
        [TestCase(" 9 4 ", null)]
        [TestCase(" 9-4 ", null)]
        [TestCase(" a94 ", null)]
        [TestCase(" _94 ", null)]
        [TestCase(" -94 ", -94)]
        [TestCase("-94", -94)]
        [TestCase("-1", -1)]
        [TestCase("0", 0)]
        [TestCase("-0", 0)]

        public void ToIntOrDefault(string str, int? expected = null, int? defaultValue = null) 
        {
            int? result = str.ToIntOrDefault(defaultValue);
            result.Should().Be(expected);
        }

        [Test]
        [TestCase("9", @"\d", 9)]
        [TestCase("95", @"\d", 9)]
        [TestCase("95", @"\d*", 95)]
        [TestCase("ninty", @"\d*", null)]
        [TestCase("ninty", @"\d*", 0, 0)]
        [TestCase("ninty", @"\d*", 90, 90)]
        [TestCase("9ninty", @"\d*", 9)]
        [TestCase(" 9 ", @"\d*", null)]
        [TestCase(" 9 ", @".*", 9)]

        public void ToIntOrDefault_Overloaded(string str, string regex_exp, int? expected = null, int? defaultValue = null)
        {
            Regex regex = new Regex(regex_exp);
            int? result = str.ToIntOrDefault(regex, defaultValue);
            result.Should().Be(expected);
        }

        [Test]
        [TestCase("9", 9, 0)]
        [TestCase("95", 95, 0)]
        [TestCase("ninty", 2, 2)]
        [TestCase("ninty", 0, 0)]
        [TestCase("9ninty", 5, 5)]
        [TestCase("9ninty", 0, 0)]
        [TestCase("9ninty", -1, -1)]
        [TestCase("9ninty", 9, 9)]
        [TestCase(" 9 ", 9, 6548)]
        [TestCase(" 9 4 ", 5467, 5467)]
        [TestCase(" 9-4 ", 7894, 7894)]
        [TestCase(" a94 ", 12345, 12345)]
        [TestCase(" _94 ", 123456, 123456)]
        [TestCase(" -94 ", -94, 123)]
        [TestCase("-94", -94, 0)]
        [TestCase("-1", -1, 0)]
        [TestCase("0", 0, 12)]
        [TestCase("-0", 0, 141)]

        public void ToIntOrDefaultInt(string str, int expected, int defaultValue)
        {
            int result = str.ToIntOrDefaultInt(defaultValue);
            result.Should().Be(expected);
        }

        [Test]
        [TestCase("9", @"\d", 9, 0)]
        [TestCase("95", @"\d", 9, 654)]
        [TestCase("95", @"\d*", 95, 0)]
        [TestCase("ninty", @"\d*", 569147, 569147)]
        [TestCase("ninty", @"\d*", 0, 0)]
        [TestCase("ninty", @"\d*", 90, 90)]
        [TestCase("9ninty", @"\d*", 9, 987654)]
        [TestCase(" 9 ", @"\d*", 684765, 684765)]
        [TestCase(" 9 ", @".*", 9, 0)]

        public void ToIntOrDefaultInt_Overloaded(string str, string regex_exp, int expected, int defaultValue)
        {
            Regex regex = new Regex(regex_exp);
            int result = str.ToIntOrDefaultInt(regex, defaultValue);
            result.Should().Be(expected);
        }

        [Test]
        [TestCase("9", 9)]
        [TestCase("95", 95)]
        [TestCase("ninty", null)]
        [TestCase("ninty", null)]
        [TestCase("9ninty", null)]
        [TestCase("9ninty", null)]
        [TestCase(" 9 ", 9)]
        [TestCase(" 9 4 ", null)]
        [TestCase(" 9-4 ", null)]
        [TestCase(" a94 ", null)]
        [TestCase(" _94 ", null)]
        [TestCase(" -94 ", -94)]

        public void ToIntOrNull(string str, int? expected = null)
        {
            int? result = str.ToIntOrNull();
            result.Should().Be(expected);
        }

        [Test]
        [TestCase("9", @"\d", 9)]
        [TestCase("95", @"\d", 9)]
        [TestCase("95", @"\d*", 95)]
        [TestCase("ninty", @"\d*", null)]
        [TestCase("ninty", @"\d*", null)]
        [TestCase("ninty", @"\d*", null)]
        [TestCase("9ninty", @"\d*", 9)]
        [TestCase(" 9 ", @"\d*", null)]
        [TestCase(" 9 ", @".*", 9)]

        public void ToIntOrNull_Overloaded(string str, string regex_exp, int? expected = null)
        {
            Regex regex = new Regex(regex_exp);
            int? result = str.ToIntOrNull(regex);
            result.Should().Be(expected);
        }

        [Test]
        [TestCase("9", 9)]
        [TestCase("95", 95)]
        [TestCase("ninty", 0)]
        [TestCase("ninty", 0)]
        [TestCase("9ninty", 0)]
        [TestCase("9ninty", 0)]
        [TestCase(" 9 ", 9)]
        [TestCase(" 9 4 ", 0)]
        [TestCase(" 9-4 ", 0)]
        [TestCase(" a94 ", 0)]
        [TestCase(" _94 ", 0)]
        [TestCase(" -94 ", -94)]

        public void ToIntOrZero(string str, int expected = 0)
        {
            int result = str.ToIntOrZero();
            result.Should().Be(expected);
        }

        [Test]
        [TestCase("9", @"\d", 9)]
        [TestCase("95", @"\d", 9)]
        [TestCase("95", @"\d*", 95)]
        [TestCase("ninty", @"\d*", 0)]
        [TestCase("ninty", @"\d*", 0)]
        [TestCase("ninty", @"\d*", 0)]
        [TestCase("9ninty", @"\d*", 9)]
        [TestCase(" 9 ", @"\d*", 0)]
        [TestCase(" 9 ", @".*", 9)]

        public void ToIntOrZero_Overloaded(string str, string regex_exp, int expected = 0)
        {
            Regex regex = new Regex(regex_exp);
            int result = str.ToIntOrZero(regex);
            result.Should().Be(expected);
        }
    }
}