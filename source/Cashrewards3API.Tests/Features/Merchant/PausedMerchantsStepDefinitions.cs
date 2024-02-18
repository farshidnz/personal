using Cashrewards3API.Common;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Tests.Helpers;
using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using FluentAssertions;
using Cashrewards3API.Features.Merchant.Models;

namespace Cashrewards3API.Tests.Features.Merchant
{
    [Binding]
    public class PausedMerchantsStepDefinitions
    {
        private static MerchantBundleServiceTests _merchantBundleServiceTests = new MerchantBundleServiceTests();
        private static MerchantBundleService _merchantBundleService;
        private int _mearchantId;
        private MerchantBundleDetailResultModel result;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            _merchantBundleService = _merchantBundleServiceTests.GetMerchantBundleService();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Nothing TODO
        }

        [Given(@"merchant is online and feature flag for Merchant Pause is '([^']*)'")]
        public void GivenMerchantIsOnlineAndFeatureFlagForMerchantPauseIs(string flagStatus)
        {
            _mearchantId = _merchantBundleServiceTests.OnlineMerchantId;
            _merchantBundleServiceTests.MockFeatureToggle(flagStatus.Equals("on", StringComparison.InvariantCultureIgnoreCase));
        }

        [When(@"merchant details for merchant id is called and Merchant Paused status is '([^']*)'")]
        public async Task WhenMerchantDetailsForMerchantIdIsCalledAndMerchantPausedStatusIs(string pauseStatus)
        {
            _merchantBundleServiceTests.OnlineStore.IsPaused = pauseStatus.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            result = await _merchantBundleService.GetMerchantBundleByIdAsync(Constants.Clients.CashRewards, _mearchantId, Constants.Clients.Blue, true);
        }


        [Then(@"the merchant commission value is '([^']*)' and commission string value is '([^']*)' and merchant tiers value is '([^']*)'")]
        public void ThenTheMerchantCommissionValueIsAndCommissionStringValueIsAndMerchantTiersValueIs(string commission, string commissionString, string merchantTiers)
        {
            result.Should().NotBeNull();
            result.Online.Should().NotBeNull();

            if (StepDefinitions.IsGreaterThanZeroString(commission))
                result.Online.Commission.Should().BeGreaterThan(0);
            else
                if (int.TryParse(commission, out var intCommission))
                    result.Online.Commission.Should().Be(intCommission);

            if (StepDefinitions.IsEmptyString(commissionString))
                result.Online.CommissionString.Should().BeEmpty();
            else
                result.Online.CommissionString.Should().NotBeEmpty();

            if (StepDefinitions.IsEmptyString(merchantTiers))
                result.Online.MerchantTiers.Should().BeEmpty();

        }
    }
}
