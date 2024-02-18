using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Features.ReferAFriend;
using Cashrewards3API.Features.ReferAFriend.Model;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.ReferAFriend
{
    public class TalkableServiceTests
    {
        private class TestState
        {
            public CommonConfig commonConfigData = new CommonConfig()
            {
                Talkable = new Talkable()
                {
                    ApiBaseAddress = "https://www.talkable.com/api/",
                    ApiKey = "abcdefg12345678",
                    Environment = "cashrewards-staging"
                }              
            };

            public TalkableService TalkableService { get; }

            public HttpClientFactoryMock HttpClientFactoryMock { get; } = new();
            public Mock<CommonConfig> commonConfig { get; }

            public TestState()
            {
                commonConfig = new Mock<CommonConfig>();

                var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Config:Talkable:ApiBaseAddress"] = "https://www.talkable.com/api/",
                    ["Config:Talkable:Environment"] = "cashrewards-staging",
                    ["Config:Talkable:ApiKey"] = "abcdefg12345678",                   
                }).Build();

                HttpClientFactoryMock.CreateClientMock("talkable", new Uri(configuration["Config:Talkable:ApiBaseAddress"]));
                HttpClientFactoryMock.SetupClientSendAsyncWithResponse("talkable", "{\"ok\": \"true\"}");

                var mapper = new MapperConfiguration(cfg => { cfg.AddProfile<RafProfile>(); cfg.AddProfile<TokenProfile>(); }).CreateMapper();

                TalkableService = new TalkableService(mapper, commonConfigData, HttpClientFactoryMock.Object);
            }
        }

        [Test]
        public async Task SignUp_ShouldReturnTrue_AndShouldCallTalkableWithSignupEvent()
        {
            var state = new TestState();

            var result = await state.TalkableService.SignUp(new TalkableSignupRequest
            {
                Email = "joe.mate@shopsalot.com",
                MemberId = 12345678,
                FirstName = "Joe",
                LastName = "Mate",
                TalkableUuid = "abc123"
            });

            result.Should().BeEquivalentTo(new TalkableSignupResult { Status = true });
            state.HttpClientFactoryMock.Requests.Should().BeEquivalentTo(new List<Request>
            {
                new Request(
                    "https://www.talkable.com/api/v2/origins",
                    JsonConvert.SerializeObject(new TalkableMemberCreateEvent {
                        SiteSlug = "cashrewards-staging",
                        Type = "Event",
                        Data = new TalkableMemberCreateEventData
                        {
                            Email = "joe.mate@shopsalot.com",
                            EventCategory = "signup",
                            EventNumber = $"referrersignup12345678",
                            Uuid = "abc123",
                            FirstName = "Joe",
                            LastName = "Mate",
                            CustomerId = "12345678"
                        }
                    }, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() } }
                    )
                )
            });
        }
    }
}