using Cashrewards3API.Features.Category;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Promotion;
using Cashrewards3API.Features.Promotion.Model;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Promotion
{
    [SetCulture("en-Au")]
    public class MemberBonusServiceTests
    {
        public class TestState
        {
            public MemberBonusService MemberBonusService { get; }

            public Mock<IPromoAppService> PromoAppService { get; }
            public Mock<IMerchantService> MerchantService { get; }
            public Mock<ICategoryService> CategoryService { get; }

            public TestState()
            {
                MerchantService = new Mock<IMerchantService>();
                MerchantService.Setup(o => o.GetMerchantsForStandardAndPremiumClients(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<HashSet<int>>()))
                    .ReturnsAsync((int clientId, int premiumClientId, HashSet<int> merchantIds) =>
                        MerchantTestData.Where(m => (m.ClientId == clientId || m.ClientId == premiumClientId) && merchantIds.Contains(m.MerchantId)));
                MerchantService.Setup(o => o.GetMerchantsForStandardClient(It.IsAny<int>(), It.IsAny<HashSet<int>>()))
                    .ReturnsAsync((int clientId, HashSet<int> merchantIds) =>
                        MerchantTestData.Where(m => m.ClientId == clientId && merchantIds.Contains(m.MerchantId)));

                PromoAppService = new Mock<IPromoAppService>();

                CategoryService = new Mock<ICategoryService>();

                MemberBonusService = new MemberBonusService(
                   PromoAppService.Object,
                   MerchantService.Object,
                   CategoryService.Object
               );
            }

            public List<MerchantViewModel> MerchantTestData { get; } =
                new List<MerchantViewModel>
                {
                    new MerchantViewModel
                    {
                        MerchantId = 123,
                        HyphenatedString = "merchant-123",
                        MerchantName = "name 123",
                        Rate = 1m,
                        Commission = 5m,
                        IsFlatRate = true,
                        TierCommTypeId = 100,
                        ClientId=1000000
                    },
                    new MerchantViewModel
                    {
                        MerchantId = 456,
                        HyphenatedString = "merchant-456",
                        MerchantName = "456",
                        Commission = 8m,
                        Rate = 1m,
                        IsFlatRate = true,
                        TierCommTypeId = 100,
                        ClientId=1000000
                    }
                };
        }

        [Test]
        public async Task GetMemberBonus_ShouldCallPromoAppWithAccessCode()
        {
            var state = new TestState();

            int clientId = 123;
            string accessCode = "code";

            state.PromoAppService.Setup(s => s.GetPromotionDetails(accessCode)).ReturnsAsync(new PromoDetailsModel());

            await state.MemberBonusService.GetMemberBonus(clientId, accessCode);

            state.PromoAppService.Verify(s => s.GetPromotionDetails(accessCode));
        }

        [Test]
        public async Task GetMemberBonus_ShouldReturnZeroValuesWhenPromotionIsNull()
        {
            var state = new TestState();

            int clientId = 123;
            string accessCode = "code";

            state.PromoAppService.Setup(s => s.GetPromotionDetails(accessCode)).ReturnsAsync(new PromoDetailsModel()
            {
                promotion = null
            });

            var result = await state.MemberBonusService.GetMemberBonus(clientId, accessCode);

            AssertMemberBonusReturnsNoValue(result);
        }

        [Test]
        public async Task GetMemberBonus_ShouldReturnZeroValuesWhenCodeNotValid()
        {
            var state = new TestState();

            int clientId = 123;
            string accessCode = "code";

            state.PromoAppService.Setup(s => s.GetPromotionDetails(accessCode)).ReturnsAsync(new PromoDetailsModel()
            {
                valid = false,
                promotion = new PromoDetailsModel.Promotion()
                {
                    bonus_value = "10"
                }
            });

            var result = await state.MemberBonusService.GetMemberBonus(clientId, accessCode);

            AssertMemberBonusReturnsNoValue(result);
        }

        [Test]
        public async Task GetMemberBonus_ShouldReturnZeroValuesWhenCodeNot200()
        {
            var state = new TestState();

            int clientId = 123;
            string accessCode = "code";

            state.PromoAppService.Setup(s => s.GetPromotionDetails(accessCode)).ReturnsAsync(new PromoDetailsModel()
            {
                code = "400",
                valid = true,
                promotion = new PromoDetailsModel.Promotion()
                {
                    bonus_value = "10"
                }
            });

            var result = await state.MemberBonusService.GetMemberBonus(clientId, accessCode);

            AssertMemberBonusReturnsNoValue(result);
        }

        private static void AssertMemberBonusReturnsNoValue(MemberBonusDto welcomeBonus)
        {
            welcomeBonus.Bonus.Should().Be(0);
            welcomeBonus.MinSpend.Should().Be(0);
            welcomeBonus.PurchaseWindow.Should().Be(0);
            welcomeBonus.ExcludeGST.Should().BeFalse();
            welcomeBonus.Category.Should().BeNull();
            welcomeBonus.Merchant.Should().BeNull();
            welcomeBonus.StoreType.Should().BeNull();
            welcomeBonus.TermsAndConditions.Should().BeNull();
        }

        [Test]
        public async Task GetMemberBonus_ShouldMapResponseFromPromoApp()
        {
            var promoDetails = new PromoDetailsModel()
            {
                code = "200",
                status = "success",
                valid = true,
                terms_and_condition = "<p>some terms and conditions</p>",
                promotion = new PromoDetailsModel.Promotion
                {
                    bonus_value = "30",
                    bonus_type = "percentage",
                    rules = new PromoDetailsModel.Rules()
                    {
                        gst = new List<string> { "accepted" },
                        member_joined_date = new PromoDetailsModel.MemberJoinedDetails()
                        {
                            after = DateTime.Parse("2021-05-19T05:21:20.517Z"),
                            before = DateTime.Parse("2021-05-28T07:21:00.000Z"),
                            required = true
                        },
                        coupon = new PromoDetailsModel.Coupon()
                        {
                            equals = "josh2",
                            required = true
                        },
                        first_purchase_window = new PromoDetailsModel.PurchaseRules()
                        {
                            max = "80",
                            required = true
                        },
                        sale_value = new PromoDetailsModel.SaleRule()
                        {
                            min = "50",
                            required = true
                        },
                        merchant_id = new PromoDetailsModel.AdditionalRule()
                        {
                            not_in = "1004078,1003469,1003776",
                            required = true
                        },
                        store_type = new PromoDetailsModel.AdditionalRule()
                        {
                            not_in = "Instore",
                            required = true
                        },
                        category_id = new PromoDetailsModel.AdditionalRule()
                        {
                            @in = "312,309",
                            required = true
                        }
                    }
                },
                reason = "Promotion associated with coupon has ended. Promotion started on 2021-05-19T05:21:21.000Z and ended on 2021-05-28T07:21:00.000Z."

            };

            var state = new TestState();
            int clientId = 123;

            state.PromoAppService.Setup(s => s.GetPromotionDetails("code")).ReturnsAsync(promoDetails);
            state.CategoryService.Setup(s => s.GetRootCategoriesAsync(clientId, null, Status.All)).ReturnsAsync(new List<CategoryDto>()
            {
                new CategoryDto()
                {
                    Id = 312,
                    Name = "CategoryA"
                },
                new CategoryDto()
                {
                    Id = 309,
                    Name = "CategoryB"
                }
            });
            state.MerchantService.Setup(s => s.GetMerchantsForStandardClient(clientId, It.Is<IEnumerable<int>>(l => l.Contains(1004078) && l.Contains(1003469) && l.Contains(1003776))))
                .ReturnsAsync(new List<MerchantViewModel>()
                {
                    new MerchantViewModel()
                    {
                        MerchantId = 1004078,
                        MerchantName = "MerchantA"
                    },
                    new MerchantViewModel()
                    {
                        MerchantId = 1003469,
                        MerchantName = "MerchantB",
                    },
                    new MerchantViewModel()
                    {
                        MerchantId = 1003776,
                        MerchantName = "MerchantC"
                    }
                });

            var bonus = await state.MemberBonusService.GetMemberBonus(clientId, "code");

            bonus.Should().NotBeNull();
            bonus.Bonus.Should().Be(decimal.Parse(promoDetails.promotion.bonus_value));
            bonus.PurchaseWindow.Should().Be(int.Parse(promoDetails.promotion.rules.first_purchase_window.max));
            bonus.MinSpend.Should().Be(decimal.Parse(promoDetails.promotion.rules.sale_value.min));
            bonus.TermsAndConditions.Should().Be(promoDetails.terms_and_condition);
            bonus.ExcludeGST.Should().Be(!promoDetails.promotion.rules.gst.Contains("accepted"));
            bonus.Category.In.Should().BeEquivalentTo(new List<string>() { "CategoryA", "CategoryB" });
            bonus.Merchant.NotIn.Should().BeEquivalentTo(new List<string>() { "MerchantA", "MerchantB", "MerchantC" });
            bonus.StoreType.NotIn.Should().BeEquivalentTo(new List<string>() { "Instore" });
        }
    }
}
