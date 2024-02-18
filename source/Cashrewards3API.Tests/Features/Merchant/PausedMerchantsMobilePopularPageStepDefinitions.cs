using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using static Cashrewards3API.Tests.Features.Merchant.PopularMerchantServiceTests;

namespace Cashrewards3API.Tests
{
    [Binding]
    public class PausedMerchantsMobilePopularPageStepDefinitions
    {
        private static TestState _testState = new TestState();
        private static PopularMerchantService _popularMerchantService;
        private PagedList<MerchantDto> result;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            _popularMerchantService = _testState.PopularMerchantService;
        }

        [Given(@"Merchant Pause Feature Flag status is '([^']*)'")]
        public void GivenMerchantPauseFeatureFlagStatusIs(string flagStatus)
        {
            _testState.MockFeatureToggle(flagStatus.Equals("on", StringComparison.InvariantCultureIgnoreCase));
        }

        [When(@"Shop page is called and Merchant Pause status is '([^']*)'")]
        public async Task WhenShopPageIsCalledAndMerchantPauseStatusIsAsync(string pauseStatus)
        {
            _testState.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 3,
                MemberComm = 100,
                RewardName = "cashback",
                IsPaused = true
            });
            _testState.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 3,
                MemberComm = 100,
                RewardName = "cashback",
                IsPaused = false
            });

            result = await _popularMerchantService.GetPopularMerchantsForMobileAsync(Constants.Clients.CashRewards, null);
        }

        [Then(@"Resulting list of merchants does '([^']*)' contain merchants with pause status true")]
        public void ThenResultingListOfMerchantsDoesContainMerchantsWithPauseStatusTrue(string display)
        {
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();

            if (display == "not")
                result.Data.Where(m => m.IsPaused).Should().BeEmpty();
        }
    }
}
