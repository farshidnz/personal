using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Features.Banners.Model;
using Cashrewards3API.Mapper;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Cashrewards3API.Tests.Mapping
{
    public class BannerProfileTests
    {
        [Test]
        public void BannerProfile_ShouldMapFieldsCorrectly()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BannerProfile>());
            var mapper = config.CreateMapper();
            var source = new Banner
            {
                Id = 123,
                Name = "BannerName",
                Status = 3,
                StartDate = new DateTime(2021, 7, 20),
                EndDate = new DateTime(2021, 12, 31),
                DesktopHtml = "DesktopHtml",
                MobileHtml = "MobileHtml",
                DesktopLink = "DesktopLink",
                MobileLink = "MobileLink",
                DesktopImageUrl = "DesktopImageUrl",
                MobileImageUrl = "MobileImageUrl",
                Position = 4,
                ClientId = Constants.Clients.Blue,
                MobileAppImageUrl = "MobileAppImageUrl",
                MobileAppLink = "MobileAppLink"
            };

            var dest = mapper.Map<BannerDto>(source);

            dest.Should().BeEquivalentTo(new BannerDto
            {
                Id = 123,
                Name = "BannerName",
                StartDate = new DateTime(2021, 7, 20),
                EndDate = new DateTime(2021, 12, 31),
                MobileLink = "MobileAppLink",
                MobileImageUrl = "MobileAppImageUrl",
                DesktopLink = "DesktopLink",
                DesktopImageUrl = "DesktopImageUrl",
                MobileBrowserLink = "MobileLink",
                MobileBrowserImageUrl = "MobileImageUrl"
            });
        }
    }
}
