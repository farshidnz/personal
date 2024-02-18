using Cashrewards3API.Common;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Tests.Features.Offers;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
namespace Cashrewards3API.Tests
{
    [Binding]
    public class PausedMerchantsStepDefinitions
    {
        private static OffersTestState TestState { get; } = new OffersTestState();
        private List<OfferViewModel> ExpectedOffers { get; set; } = new List<OfferViewModel>();
        private IEnumerable<int> ResultedOfferIds { get; set; } = Enumerable.Empty<int>();

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
        }

        [Given(@"Feature Flag for Merchant Pause Is '([^']*)'")]
        public void GivenFeatureFlagForMerchantPauseIs(string flagstatus)
        {
            TestState.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(flagstatus.ToLower() == "on");
        }

        [Given(@"one or more '([^']*)' offers are listed with Merchant Paused status as '([^']*)'")]
        public void GivenOneOrMoreOffersAreListedWithMerchantPausedStatusAs(string offertype, string pausestatus)
        {
            var isMerchantPaused = pausestatus.ToLower() == "true";

            for (int id = TestState.OffersTestData.Max(o => o.OfferId) + 1, i = 0; i < 4; i++, id++)
                ExpectedOffers.Add(NewOffer(id, offertype, isMerchantPaused));

            TestState.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            TestState.OffersTestData.AddRange(ExpectedOffers);
        }

        [When(@"all Offers for '([^']*)' of '([^']*)' are requested")]
        public async Task WhenAllOffersForOfAreRequestedAsync(string platform, string offertype)
        {
            ResultedOfferIds = (platform.ToLower(), offertype.ToLower()) switch
            {
                ("web", "special") =>
                    SpecialOfferIds(await TestState.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, Constants.Clients.Blue, null, false)),
                ("mob", "increased") => (await TestState.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, null))
                    .Select(offer => offer.Id),
                _ => throw new System.NotImplementedException(),
            };
        }

        [Then(@"the result list does '([^']*)' contain listed offers")]
        public void ThenTheResultListDoesContainListedOffers(string containassertion)
        {
            var merchantPausedOfferIds = ExpectedOffers.Select(o => o.OfferId);

            if (containassertion.Contains("not", System.StringComparison.InvariantCultureIgnoreCase))
                ResultedOfferIds.Should().NotContain(merchantPausedOfferIds);
            else
                ResultedOfferIds.Should().Contain(merchantPausedOfferIds);
        }

        private IEnumerable<int> SpecialOfferIds(Cashrewards3API.Common.Dto.SpecialOffersDto specialOffers) =>
            specialOffers.CashBackIncreasedOffers.Select(offer => offer.Id).Union(specialOffers.PremiumFeatureOffers.Select(offer => offer.Id));

        private OfferViewModel NewOffer(int offerId, string offertype, bool isMerchantPaused)
        {
            var clientId = Constants.Clients.CashRewards;
            bool isFeatured = true;
            bool isCashbackIncreased = true;
            bool isPremium = false;

            switch (offertype.ToLower())
            {
                case "special":
                    if (offerId % 2 == 0)
                    {
                        clientId = Constants.Clients.Blue;
                        isPremium = true;
                        isCashbackIncreased = false;
                    }
                    break;
                case "increased":
                    isCashbackIncreased = true;
                    break;
            }

            return new OfferViewModel
            {
                OfferId = offerId,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = clientId,
                IsFeatured = isFeatured,
                IsPremiumFeature = isPremium,
                IsCashbackIncreased = isCashbackIncreased,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = isMerchantPaused
            };
        }
    }
}
