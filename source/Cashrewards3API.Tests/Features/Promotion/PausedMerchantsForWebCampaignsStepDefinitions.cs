using Cashrewards3API.Common;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Features.Promotion.Model;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Cashrewards3API.Tests.Features.Promotion
{
    [Binding]
    public class PausedMerchantsForWebCampaignsStepDefinitions
    {
        private PromotionServiceTests.TestState _testState = new PromotionServiceTests.TestState(mockStrapi: false);
        private PromotionDto _promotionDefinition;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
        }

        [Given(@"the feature flag for Merchant Pause is '([^']*)'")]
        public void GivenTheFeatureFlagForMerchantPauseIs(string flagStatus)
        {
            _testState.PausedMerchantFeatureToggle.IsFeatureEnabled = StepDefinitions.IsTrue(flagStatus);
        }
            
        [Given(@"paused status for merchant (.*) is '([^']*)'")]
        public void GivenPausedStatusForMerchantIs(int merchantId, string pauseStatus)
        {
            _testState.MerchantTestData.Add(new MerchantViewModel()
            {
                MerchantId = merchantId,
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = $"Merchant-{merchantId}",
                MerchantName = merchantId.ToString(),
                Rate = 1m,
                Commission = 5m,
                IsFlatRate = true,
                TierCommTypeId = 100,
                IsPaused = StepDefinitions.IsTrue(pauseStatus),
            });

            // add other merchants corresponding to items in categories
            var promotionDefinition = JsonConvert.DeserializeObject<PromotionDefinition>(_testState.PromotionDefinition,
               new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
            var itemMerchants = promotionDefinition.Categories.SelectMany(c => c.Items.Select(i => i.ItemId)).Distinct();
            foreach (var itemId in itemMerchants.Where(i => i != merchantId))
                _testState.MerchantTestData.Add(new MerchantViewModel()
                {
                    MerchantId = itemId,
                    ClientId = Constants.Clients.CashRewards,
                    HyphenatedString = $"Merchant-{itemId}",
                    MerchantName = itemId.ToString(),
                    Rate = 1m,
                    Commission = 5m,
                    IsFlatRate = true,
                    TierCommTypeId = 100,
                    IsPaused = false
                });
        }

        [Given(@"paused status for offer id (.*) with merchant id (.*) is '([^']*)'")]
        public void GivenPausedStatusForOfferIdWithMerchantIdIs(int offerId, int merchantId, string pauseStatus)
        {
            _testState.OfferTestData.Add(new OfferDto
            {
                Id = offerId,
                MerchantId = merchantId,
                HyphenatedString = "groupon-offer-4",
                ClientCommissionString = "4% cashback",
                IsFeatured = false,
                IsCashbackIncreased = false,
                IsMerchantPaused = StepDefinitions.IsTrue(pauseStatus),
            });
        }
       
        [When(@"promotions are requested")]
        public async Task WhenPromotionsAreRequestedAsync()
        {
            _promotionDefinition = await _testState.PromotionService.GetPromotionInfo(Constants.Clients.CashRewards, Constants.Clients.Blue, "mothers-day");
        }

        [Then(@"merchant id (.*) and offer id (.*) should '([^']*)' be in the result")]
        public void ThenMerchantIdAndOfferIdShouldBeInTheResult(int merchantId, int offerId, string inResult)
        {
            _promotionDefinition.Should().NotBeNull();
            _promotionDefinition.Categories.Should().NotBeNull();
            // this tests merchants
            _promotionDefinition.Categories.Any(c => ContainsItem(merchantId, c.Items)).Should().Be(StepDefinitions.IsTrue(inResult));
            // this tests offers
            _promotionDefinition.Categories.Any(c => ContainsItem(offerId, c.Items)).Should().Be(StepDefinitions.IsTrue(inResult));
        }

        private bool ContainsItem(int merchantId, List<PromotionCategoryItemData> itemData) =>
           itemData != null && itemData.Any(c => c.ItemId == merchantId);
    }
}
