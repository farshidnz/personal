using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.ShopGoClient;
using Cashrewards3API.Features.ShopGoClient.Models;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.ShopGoClient
{
    [TestFixture]
    public class ShopGoClientServiceTest
    {
        public class TestState
        {

            public ShopGoClientService shopGoClientService;
            public TestState()
            {
                var repository = new Mock<IReadOnlyRepository>();
                repository.Setup(r => r.QueryAsync<ShopGoClientModel>(It.IsAny<string>(), It.IsAny<object>()))
                                        .ReturnsAsync(TestData());

                var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfiles>())
                                    .CreateMapper();
                var redisUtil = new RedisUtilMock();
                redisUtil.Setup<IEnumerable<ShopGoClientModel>>();
                shopGoClientService = new ShopGoClientService(
                                                    repository.Object,
                                                    mapper,
                                                    new Mock<ICacheKey>().Object,
                                                    redisUtil.Object, 
                                                    new Mock<CacheConfig>().Object);
            }
            private IList<ShopGoClientModel> TestData()
            {
                return new List<ShopGoClientModel>() {
                    new ShopGoClientModel (){
                        ClientId = 1000000,
                        ClientName = "Cashrewards",
                        ClientKey = "CRW",
                        Status= 1
                    },
                    new ShopGoClientModel (){
                        ClientId = 1000034,
                        ClientName = "ANZ Bankc",
                        ClientKey = "ANZ",
                        Status= 0
                    }
                };
            }

        }

        [Test]
        public async Task GetAllShopGoClients_ShouldReturnAllClients_GivenOpenClosedClients()
        {
            var state = new TestState();
            var expectedClientIds = new List<int>() { 1000000, 1000034 };
            var result = await state.shopGoClientService.GetAllShopGoClients();
            var resultClientIds = result.Select(client => client.ClientId);
            
            resultClientIds.Should().BeEquivalentTo(expectedClientIds);
        }
    }
}
