using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using static Cashrewards3API.Tests.Features.Merchant.MerchantServiceTests;

namespace Cashrewards3API.Tests.Features.Merchant
{
    [Binding]
    public class PausedMerchantsMobileAllOffersStepDefinitions
    {
        private static TestState _testState = new TestState();
        private static MerchantService _merchantService;
        private PagedList<MerchantBundleBasicModel> result;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            _merchantService = _testState.MerchantService;
        }

        [Given(@"feature flag for Merchant Pause is '([^']*)'")]
        public void GivenFeatureFlagForMerchantPauseIs(string flagStatus)
        {
            _testState.MockFeatureToggle(StepDefinitions.IsTrue(flagStatus));
        }

        [When(@"all merchants is called and Merchant Paused status is '([^']*)'")]
        public async void WhenAllMerchantsIsCalledAndMerchantPausedStatusIs(string pauseStatus)
        {
            _testState.GivenMerchant(new MerchantFullView
             {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "test-string",
                TierCommTypeId = 101,
                ClientComm = 100,
                MemberComm = 100,
                Commission = 8,
                IsFlatRate = true,
                IsPaused = pauseStatus.Equals("true")
            });
            _testState.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "test-string",
                TierCommTypeId = 101,
                ClientComm = 100,
                MemberComm = 100,
                Commission = 8,
                IsFlatRate = true,
                IsPaused = false
            });



            result = await _testState.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards
            });
        }

        [Then(@"the resulting list does '([^']*)' contain merchants with pause status true")]
        public void ThenTheResultingListDoesContainMerchantsWithPauseStatusTrue(string display)
        {
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();

            if (display == "not")
                result.Data.Where(m => m.Online.IsPaused).Should().BeEmpty();               
        }
    }
}
