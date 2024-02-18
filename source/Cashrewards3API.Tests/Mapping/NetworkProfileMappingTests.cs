using AutoMapper;
using Cashrewards3API.Features.ShopGoNetwork.Model;
using Cashrewards3API.Mapper;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Mapping
{
    class NetworkProfileMappingTests
    {
        [Test]
        public void test()
        {

            var config = new MapperConfiguration(cfg => cfg.AddProfile<NetworkProfile>());
            var mapper = config.CreateMapper();
            var networkModel = new Network() { 
                    NetworkId = 1001,
                    NetworkName = "Network-1",
                    DeepLinkHolder = "DeepLinkHolder-1",
                    GstStatusId = 1,
                    Status = 1,
                    NetworkKey = "CRW",
                    TimeZoneId = 1001,
                    TrackingHolder = "tracking-1"
            };

            var expected = new NetworkDto()
            {
                Id = 1001,
                Name = "Network-1",
                DeepLinkHolder = "DeepLinkHolder-1",
                GstStatusId = 1,
                Status = 1,
                NetworkKey = "CRW",
                TimeZoneId = 1001,
                TrackingHolder = "tracking-1"
            };
           
            var result = mapper.Map<NetworkDto>(networkModel);
            result.Should().BeEquivalentTo(expected);
        }
    }
}
