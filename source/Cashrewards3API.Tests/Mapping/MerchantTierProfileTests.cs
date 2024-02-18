using AutoMapper;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Mapper;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Mapping
{
    [TestFixture]
    public class MerchantTierProfileTests
    {

        [TestCase(true)]
        [TestCase(false)]
        public void MerchantStoreTierMapsToMerchantTierResultModel(bool withPremium)
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MerchantTierProfile>());
            var mapper = config.CreateMapper();

            var tier = new MerchantStore.Tier()
            {
                Id = 123,
                Name = "name",
                ClientCommission = 10,
                CommissionType = "percent",
                EndDateTime = "sometime",
                TierSpecialTerms = "terms",
                Exclusions = "ex",
                Premium = withPremium ? new PremiumTier()
                {
                    ClientCommission = 20,
                    ClientCommissionString = "20 %",
                    CommissionType = "percent"
                } : null
            };

            var result = mapper.Map<MerchantTierResultModel>(tier);

            result.Id.Should().Be(123);
            result.Name.Should().Be("name");
            result.Commission.Should().Be(10);
            result.CommissionType.Should().Be("percent");
            result.EndDateTime.Should().Be("sometime");
            result.Terms.Should().Be("terms");
            result.Exclusions.Should().Be("ex");

            if (withPremium)
            {
                result.Premium.Commission.Should().Be(20);
                result.Premium.CommissionType.Should().Be("percent");
            }
            else
            {
                result.Premium.Should().BeNull();
            }




        }

    }
}
