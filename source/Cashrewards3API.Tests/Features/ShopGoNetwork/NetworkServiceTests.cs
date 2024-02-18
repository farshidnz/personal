using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.ShopGoNetwork.Model;
using Cashrewards3API.Features.ShopGoNetwork.Repository;
using Cashrewards3API.Features.ShopGoNetwork.Service;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.ShopGoNetwork
{
    [TestFixture]
    public class NetworkServiceTests
    {

        public class TestState
        {
            public NetworkService networkService;

            public List<Network> NetworkModels { get; } = new List<Network>()
            {
                new Network() {
                    NetworkId = 1001,
                    NetworkName = "Network-1",
                    DeepLinkHolder = "DeepLinkHolder-1",
                    GstStatusId = 1,
                    Status = 1,
                    NetworkKey = "CRW",
                    TimeZoneId = 1001,
                    TrackingHolder = "tracking-1"
                },
                new Network()
                {
                    NetworkId = 1002,
                    NetworkName = "Network-2",
                    DeepLinkHolder = "DeepLinkHolder-2",
                    GstStatusId = 1,
                    Status = 1,
                    NetworkKey = "CRS",
                    TimeZoneId = 1002,
                    TrackingHolder = "tracking-2"
                }
            };

            public TestState()
            {
                var config = new MapperConfiguration(cfg => cfg.AddProfile<NetworkProfile>());
                var mapper = config.CreateMapper();
                var cacheKey = new Mock<ICacheKey>();
                cacheKey.Setup(svc => svc.GetNetworkKey()).Returns("123");
                var redisUtil = new Mock<IRedisUtil>();
                var cacheConfig = new Mock<CacheConfig>();
                var networkRepository = new Mock<INetworkRepository>();
                networkRepository.Setup(rep => rep.GetNetworks()).ReturnsAsync(NetworkModels);
                networkService = new NetworkService(
                    mapper, 
                    cacheKey.Object,
                    new RedisUtilMock()
                        .Setup<IEnumerable<NetworkDto>>()
                        .Object, 
                    cacheConfig.Object, 
                    networkRepository.Object);

            }
        }

        [Test]
        public async Task GetNetworks_ShouldReturnNetworkDtos()
        {
            var state = new TestState();
            var data = new List<NetworkDto>()
                {
                    new NetworkDto()
                    {
                        Id = 1001,
                        Name = "Network-1",
                        DeepLinkHolder = "DeepLinkHolder-1",
                        GstStatusId = 1,
                        Status = 1,
                        NetworkKey = "CRW",
                        TimeZoneId = 1001,
                        TrackingHolder = "tracking-1"
                    },
                    new NetworkDto()
                    {
                        Id = 1002,
                        Name = "Network-2",
                        DeepLinkHolder = "DeepLinkHolder-2",
                        GstStatusId = 1,
                        Status = 1,
                        NetworkKey = "CRS",
                        TimeZoneId = 1002,
                        TrackingHolder = "tracking-2"
                    }
                };

            var exprected = new PagedList<NetworkDto>(2, 2, data);
            
            var result = await state.networkService.GetNetworks(new NetworkFilterRequest{Limit = 2 });
            result.Should().BeEquivalentTo(exprected);
        }

        

    }
}
