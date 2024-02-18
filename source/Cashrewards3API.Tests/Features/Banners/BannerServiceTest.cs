using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Extensions;
using Cashrewards3API.Features.Banners.Model;
using Cashrewards3API.Features.Banners.Service;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Banners
{
    [TestFixture]
    public class BannerServiceTest
    {
        private class TestState
        {
            public BannerService bannerService;

            private readonly Mock<ICacheKey> _cacheKey;
            private Mock<IReadOnlyRepository> _repository { get; set; }
            private readonly Mock<CacheConfig> _cacheConfig;

            public List<Banner> BannerModels { get; } = new List<Banner>()
            {
                new Banner()
                {
                    Clients = new List<int>(){Constants.Clients.CashRewards,Constants.Clients.Blue},
                    Position =1,
                    Name = "Banner FOR CASHREWARDS, LoggedOut, ANZ",
                    Status = 1,
                },
                new Banner()
                {
                    Clients = new List<int>(){Constants.Clients.CashRewards},
                    Position =1,
                    Name = "Banner FOR CASHREWARDS AND LOGGED OUT ONLY",
                    Status = 1,
                },
                 new Banner()
                {
                    Clients = new List<int>(){Constants.Clients.CashRewards},
                    Position =1,
                    Name = "Banner FOR CASHREWARDS AND LOGGED OUT ONLY 2",
                    Status = 1,
                },
                 new Banner()
                {
                    Clients = new List<int>(){Constants.Clients.Blue},
                    Position =1,
                    Name = "Banner FOR ANZ Only",
                    Status = 1,
                }
            };

            public TestState()
            {
                var cacheConfig = new CacheConfig()
                {
                    CategoryDataExpiry = 1
                };

                var mapper = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<BannerProfile>();
                }).CreateMapper();

                _cacheKey = new Mock<ICacheKey>();
                _cacheConfig = new Mock<CacheConfig>();

                var redisUtil = new RedisUtilMock();
                redisUtil.Setup<IEnumerable<Banner>>();
                var dateTimeProvider = new IDateTimeProviderMock();
                dateTimeProvider.Setup<System.DateTime>();

                bannerService = new BannerService(mapper, redisUtil.Object, _cacheKey.Object, _cacheConfig.Object, dateTimeProvider.Object, CreateRepository());
            }

            public IReadOnlyRepository CreateRepository()
            {
                _repository = new Mock<IReadOnlyRepository>();
                _repository.Setup(o => o.QueryAsync<Banner, int, Banner>(It.IsAny<string>(), It.IsAny<Func<Banner, int, Banner>>(), It.Is<object>(obj => obj.GetPropertyValue<int>("clientId") == 1000000), It.IsAny<string>()))
                  .ReturnsAsync(() => BannerModels.Where(o => o.Clients.Contains(1000000)).ToList());
                _repository.Setup(o => o.QueryAsync<Banner, int, Banner>(It.IsAny<string>(), It.IsAny<Func<Banner, int, Banner>>(), It.Is<object>(obj => obj.GetPropertyValue<int>("clientId") == 1000034), It.IsAny<string>()))
                  .ReturnsAsync(() => BannerModels.Where(o => o.Clients.Contains(1000034)).ToList());
                return _repository.Object;
            }
        }

        [Test]
        public async Task GetBanners_ForPremiumPerson_ShouldReturnPremiumBanner()
        {
            var state = new TestState();
            IEnumerable<Banner> banners = await state.bannerService.GetBannersFromClientId(Constants.Clients.Blue);
            banners.Where(b => b.Clients.Contains(Constants.Clients.Blue)).Count().Should().Be(2);
        }

        [Test]
        public async Task GetBanners_ForCashRewardsAndLoggedOutPerson_ShouldReturnCashRewardsBanners()
        {
            var state = new TestState();
            IEnumerable<Banner> banners = await state.bannerService.GetBannersFromClientId(Constants.Clients.CashRewards);
            banners.Where(b => b.Clients.Contains(Constants.Clients.CashRewards)).Count().Should().Be(3);
        }
    }
}