using Cashrewards3API.Features.Promotion;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Promotion
{
    [TestFixture]
    public class PromoAppServiceTests
    {
        private class TestState
        {
            public PromoAppService PromoAppService{ get; }

            public HttpClientFactoryMock HttpClientFactoryMock { get; } = new();

            public TestState()
            {
                var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Config:PromoApp:ApiBaseAddress"] = "https://promoapp.stg-internal.cashrewards.com.au",
                    ["Config:PromoApp:CouponValidationEndpoint"] = "/api/v3/coupons/validate/"
                }).Build();

                HttpClientFactoryMock.CreateClientMock("promoapp", new Uri(configuration["Config:PromoApp:ApiBaseAddress"]));

                var response = TestDataLoader.Load(@".\Features\Promotion\JSON\promo-coupon-response.json");

                HttpClientFactoryMock.SetupClientSendAsyncWithResponse("promoapp", response);


                PromoAppService = new PromoAppService(configuration, HttpClientFactoryMock.Object);
            }
        }

        [Test]
        public async Task SignUp_ShouldReturnTrue_AndShouldCallTalkableWithSignupEvent()
        {
            var state = new TestState();

            var result = await state.PromoAppService.GetPromotionDetails("accessCode");

            result.Should().NotBeNull();
            result.code.Should().Be("200");
            result.terms_and_condition.Should().Be("<p>some terms and conditions</p>");
            result.status.Should().Be("success");
            result.promotion.bonus_type.Should().Be("percentage");
            result.promotion.bonus_value.Should().Be("30");
            result.promotion.rules.member_joined_date.after.Should().Be(new DateTime(2021, 5, 19, 5, 21, 20, 517));
            result.promotion.rules.member_joined_date.before.Should().Be(new DateTime(2021, 5, 28, 7, 21, 0));
            result.promotion.rules.coupon.equals.Should().Be("josh2");
            result.promotion.rules.gst.Should().BeNull();
            result.promotion.rules.sale_value.min.Should().Be("50");
            result.promotion.rules.first_purchase_window.max.Should().Be("80");
            result.promotion.rules.category_id.@in.Should().Be("312,309");
            result.promotion.rules.store_type.not_in.Should().Be("Instore");
            result.promotion.rules.merchant_id.not_in.Should().Be("1004078,1003469,1003776");

            state.HttpClientFactoryMock.Requests.Count.Should().Be(1);
            state.HttpClientFactoryMock.Requests.Single().RequestUri
                .Should().Be("https://promoapp.stg-internal.cashrewards.com.au/api/v3/coupons/validate/accessCode");


        }

    }
}
