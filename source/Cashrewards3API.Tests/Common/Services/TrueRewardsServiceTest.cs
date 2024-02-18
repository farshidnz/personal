using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Model;
using Cashrewards3API.Features.Member.Dto;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Common.Services
{
    public class TrueRewardsServiceTest
    {
        public TRAuthTokenDTO tokenMock = new TRAuthTokenDTO() { authToken = "theToken" };

        private class TestState
        {
            public CommonConfig commonConfigData = new CommonConfig()
            {
                TrueRewards = new TrueRewards()
                {
                    App = "store",
                    TokenIssuer = "https://truerewards.com.au",
                    ApiKey = "F040B97A-A9BE-4439-8970-7E43AB94F5BA"
                }
            };

            public TRAuthTokenDTO tokenMock = new TRAuthTokenDTO() { authToken = "theToken" };

            public TrueRewardsService trueRewardsService;
            public HttpClientFactoryMock HttpClientFactoryMock { get; } = new();
            public Mock<CommonConfig> commonConfig { get; }

            public TestState()
            {
                commonConfig = new Mock<CommonConfig>();

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Config:TrueRewards:App"] = "TrueRewardsApp",
                    ["Config:TrueRewards:ApiKey"] = "123456",
                    ["Config:TrueRewards:TokenIssuer"] = "https://truerewards.com.au"
                }).Build();

                HttpClientFactoryMock.CreateClientMock("truerewards", new Uri(configuration["Config:TrueRewards:TokenIssuer"]));
                HttpClientFactoryMock.SetupClientSendAsyncWithResponse("truerewards", JsonConvert.SerializeObject(tokenMock));

                var mapper = new MapperConfiguration(cfg => { cfg.AddProfile<TokenProfile>(); }).CreateMapper();

                trueRewardsService = new TrueRewardsService(mapper, commonConfigData, HttpClientFactoryMock.Object);
            }
        }

        [Test]
        public async Task GetToken_ShouldReturnTrue_AndShouldCallRewardsWithGetToken()
        {
            var state = new TestState();
            state.HttpClientFactoryMock.SetupClientSendAsyncWithResponse("truerewards", JsonConvert.SerializeObject(tokenMock));
            var result = await state.trueRewardsService.GetToken(new Cashrewards3API.Common.Model.AuthnRequestContext
            {
                FullName = "ignacio Barreto",
                Email = "joe.mate@shopsalot.com",
            });

            result.Should().BeEquivalentTo(new TokenContext { AuthToken = "theToken" });
            var req = state.HttpClientFactoryMock.Requests.Single();
            req.RequestUri.Should().Be("https://truerewards.com.au/API/fetch-tr-widget-auth");
            req.Body.Should().Be("apiKey=F040B97A-A9BE-4439-8970-7E43AB94F5BA&app=store&name=ignacio+Barreto&email=joe.mate%40shopsalot.com");
        }
    }
}