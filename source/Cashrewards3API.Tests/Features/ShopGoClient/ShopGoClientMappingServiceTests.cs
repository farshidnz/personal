using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.ShopGoClient;
using Cashrewards3API.Features.ShopGoClient.Models;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace Cashrewards3API.Tests.Features.ShopGoClient
{
    [TestFixture]
    public class ShopGoClientMappingServiceTests
    {
         private ShopGoClientService _shopGoClientService;

        [SetUp] 
        public void SetUp()
        {
            var repository = new Mock<IReadOnlyRepository>();
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfiles>())
                                .CreateMapper();
            var redisUtil = new RedisUtilMock();
            redisUtil.Setup<IEnumerable<ShopGoClientModel>>();
            _shopGoClientService = new ShopGoClientService(
                                                    repository.Object,
                                                    mapper,
                                                    new Mock<ICacheKey>().Object,
                                                    redisUtil.Object,
                                                    new Mock<CacheConfig>().Object);
        }

        [Test]
        public void ConvertToShopGoResultModel_ShouldConvertShopGoClientModelToResultModel()
        {
            var clients = new List<ShopGoClientModel>(){
                new ShopGoClientModel (){
                    ClientId = 1000000,
                    ClientName = "Cashrewards",
                    ClientKey = "CRW",
                    Status= 1
                }
            };

            var clientResultModels = _shopGoClientService.ConvertToResultModel(clients);
            var client = clientResultModels.Single();

            client.ClientId.Should().Be(1000000);
            client.ClientName.Should().Be("Cashrewards");
            client.ClientKey.Should().Be("CRW");
            client.Status.Should().Be(1);
        }    
    }
}