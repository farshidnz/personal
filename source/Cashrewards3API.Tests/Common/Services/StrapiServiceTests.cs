using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Features.Promotion.Model;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Common.Services
{
    public class StrapiServiceTest
    {
        const string v3Uri = "https://strapi.stg.cashrewards.com.au/";
        const string v4Uri = "https://strapiv4.uat.aws.cashrewards.com.au/";

        private class TestState
        {
            public StrapiService StrapiService { get; }
            public HttpClientFactoryMock HttpClientFactoryMock { get; } = new();

            public TestState(Action<HttpClientFactoryMock, Dictionary<string, string>> beforeSUTCreate = null)
            {
                HttpClientFactoryMock.CreateClientMock("strapi", new Uri(v3Uri));
                HttpClientFactoryMock.CreateClientMock("strapiv4", new Uri(v4Uri));

                var configDict = new Dictionary<string, string>();

                beforeSUTCreate?.Invoke(HttpClientFactoryMock, configDict);

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(configDict)
                    .Build();

                StrapiService = new StrapiService(HttpClientFactoryMock.Object, configuration);
            }
        }

        [Test]
        public async Task GetCampaign_ShouldReturnStrapiCampaign_GivenCampaignExists()
        {
            var state = new TestState();
            state.HttpClientFactoryMock.SetupClientSendAsyncWithJsonResponse("strapi", TestDataLoader.Load<IEnumerable<StrapiCampaign>>(@".\Common\Services\JSON\mothers-day.strapi.json"));

            var campaign = await state.StrapiService.GetCampaign("mothers-day");

            campaign.Title.Should().Be("Super gifts for Super Mums");
        }

        [Test]
        public void ShouldCreateStrapiv4Client_WhenFeatureSwitchedToUseStrapiv4()
        {
            var clientCalled = string.Empty;
            var state = new TestState((hc, c) =>
            {
                hc.Setup(x => x.CreateClient(It.IsAny<string>()))
                  .Callback<string>((s) => clientCalled = s);

                c["UseStrapiV4"] = "true";
            });

            clientCalled.Should().Be("strapiv4");
        }

        [Test]
        public async Task WhenStrapiv3_GetCampaign_ShouldUsev3Filters()
        {
            var state = new TestState();

            state.HttpClientFactoryMock.SetupClientGetRequests("strapi", HttpStatusCode.OK);

            var slugName = "someslug";
            await state.StrapiService.GetCampaign(slugName);

            state.HttpClientFactoryMock.Requests[0].RequestUri.Should().Be($"{v3Uri}campaigns?_where[slug]={slugName}");
        }

        [Test]
        public async Task WhenStrapiv4_GetCampaign_ShouldUsev4Filters()
        {
            var state = new TestState((hc, c) =>
            {
                c["UseStrapiV4"] = "true";
            });

            state.HttpClientFactoryMock.SetupClientGetRequests("strapiv4", HttpStatusCode.OK);

            var slugName = "someslug";
            await state.StrapiService.GetCampaign(slugName);

            state.HttpClientFactoryMock.Requests[0].RequestUri.Should().Be($"{v4Uri}campaigns?filters[slug][$eq]={slugName}");
        }

        [Test]
        public async Task WhenStrapiv4_GetCampaign_ShouldStillReturnBackStrapiCampaignStructure()
        {
            var state = new TestState((hc, c) =>
            {
                c["UseStrapiV4"] = "true";
            });

            state.HttpClientFactoryMock.SetupClientSendAsyncWithJsonResponse("strapiv4", TestDataLoader.Load<StrapiCampaign>(@".\Common\Services\JSON\strapi-v4.json"));

            var campaign = await state.StrapiService.GetCampaign("style-drop");

            campaign.Title.Should().Be("StyleDrop");

            // also check the child classes also work as expected
            campaign.Banner_Image.Desktop.Url.Should().Be("https://system.aws.cashrewards.com.au/strapi-assets/2022_07_20_Fashion_Event_Campaign_15_Jul_2022_102604_4860f0ab62.gif");

            // categories
            campaign.Category[0].Title.Should().Be("18 Title");
            campaign.Category[0].Item[0].Merchant_Id.Should().Be(1004881);
            campaign.Category[0].Item[0].Background_Image.Url.Should().Be("https://system.aws.cashrewards.com.au/strapi-assets/h_and_m_bc81443869.png");

            // campaignsections
            campaign.Campaign_Section.Large_Head_Image.Url.Should().Be("https://system.aws.cashrewards.com.au/strapi-assets/cashrewards_the_internet_fashion_trends_73577f53f2.jpg");

            // seo
            campaign.Seo.Meta_Title.Should().Be("Style DROP | Step into a new style, with our fashion event | Cashrewards 20-21 July");

            // campaigns
            campaign.Campaigns[0].Title.Should().Be("testcampaign");
            campaign.Campaigns[0].Offers[0].Offer_Title.Should().Be("testcampaign offer");
            campaign.Campaigns[0].Offers[0].Offer_Image.Url.Should().Be("https://system.aws.cashrewards.com.au/strapi-assets/cashrewards_the_internet_fashion_trends_73577f53f2.jpg");
            campaign.Campaign_Section.Order.Should().Be("top");
        }

        [Test]
        public async Task Strapiv3_GetCampaign_WhenNoResults_ShouldReturnBackNullStrapiCampaign()
        {
            var state = new TestState();

            state.HttpClientFactoryMock.SetupClientSendAsyncWithJsonResponse("strapi", Array.Empty<dynamic>());

            var campaign = await state.StrapiService.GetCampaign("nonexistant");

            campaign.Should().BeNull();
        }

        [Test]
        public async Task Strapiv4_GetCampaign_WhenNoResults_ShouldReturnBackNullStrapiCampaign()
        {
            var state = new TestState((hc, c) =>
            {
                c["UseStrapiV4"] = "true";
            });

            state.HttpClientFactoryMock.SetupClientSendAsyncWithJsonResponse("strapiv4", new
            {
                data = Array.Empty<dynamic>(),
                meta = new
                {
                    page = 1,
                    pageSize = 25,
                    pageCount = 0,
                    total = 0
                }
            });

            var campaign = await state.StrapiService.GetCampaign("nonexistant");

            campaign.Should().BeNull();
        }

        [Test]
        public async Task Strapiv4_GetCampaign_WhenNoResultsIncludingNullData_ShouldReturnBackNullStrapiCampaign()
        {
            var state = new TestState((hc, c) =>
            {
                c["UseStrapiV4"] = "true";
            });

            state.HttpClientFactoryMock.SetupClientSendAsyncWithJsonResponse("strapiv4", new
            {
            });

            var campaign = await state.StrapiService.GetCampaign("nonexistant");

            campaign.Should().BeNull();
        }
    }
}
