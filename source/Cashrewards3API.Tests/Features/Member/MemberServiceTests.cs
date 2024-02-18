using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Member.Repository;
using Cashrewards3API.Features.Member.Service;
using Cashrewards3API.Features.Person.Interface;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Member
{
    [SetCulture("en-Au")]
    public class MemberServiceTests
    {
        private class TestState
        {
            public MemberService MemberService { get; set; }

            public TestState()
            {
                var mockRepository = new Mock<IRepository>();
                mockRepository.Setup(s =>
                        s.QueryFirstOrDefault<MemberModel>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(MemberModelTestData);

                var config = new MapperConfiguration(cfg => cfg.AddProfile<MemberProfile>());
                var mapper = config.CreateMapper();

                MemberService = new MemberService(
                    mockRepository.Object,
                    mockRepository.Object,
                    mapper,
                    Mock.Of<IEncryption>(),
                    Mock.Of<IPerson>(),
                    Mock.Of<IMemberRepository>(),
                    Mock.Of<ICacheKey>(),
                    Mock.Of<IRedisUtil>(),
                    Mock.Of<CacheConfig>(),
                    Mock.Of<ITokenService>(),
                    Mock.Of<IDateTimeProvider>()
                );
            }

            private MemberModel MemberModelTestData => TestDataLoader.Load<MemberModel>("Features/Member/JSON/MemberInfoResponse.json");
        }

        [Test]
        public async Task GetMemberById_ShouldReturnMemberDto_GivenMemberModel()
        {
            var state = new TestState();

            var result = await state.MemberService.GetMemberById(123);

            var expectMember = new MemberDto()
            {
                FirstName = "FirstName",
                LastName = "LastName",
                MemberId = 1001112621,
                MemberNewId = new Guid("c28c59b2-95b4-4c27-a5da-42453f94962e"),
                AccessCode = "",
                Email = "test@cashrewards.com",
                ClientId = 1000000,
                DateJoined = DateTime.Parse("2021-05-21T17:41:44.767"),
                OriginationSource = "anz",
                PostCode = "2077",
                Mobile = "+61 449999999",
                CampaignId = null,
                PremiumStatus = 1,
                ReceiveNewsLetter = true,
            };

            result.Should().BeEquivalentTo(expectMember);
        }

        [Test]
        public async Task GetMemberByEmail_ShouldReturnMemberDto_GivenMemberModel()
        {
            var state = new TestState();

            var result = await state.MemberService.GetMemberByEmail("test@cashrewards.com");

            var expectMember = new EmailMemberDto()
            {
                FirstName = "FirstName",
                LastName = "LastName",
                MemberId = 1001112621,
                MemberNewId = new Guid("c28c59b2-95b4-4c27-a5da-42453f94962e"),
                AccessCode = "",
                Email = "test@cashrewards.com",
                ClientId = 1000000,
                DateJoined = DateTime.Parse("2021-05-21T17:41:44.767"),
                OriginationSource = "anz",
                PostCode = "2077",
                Mobile = "+61 449999999",
                CampaignId = null,
                PremiumStatus = 1,
                ReceiveNewsLetter = true,
                Status = 0
            };

            result.Should().BeEquivalentTo(expectMember);
        }
    }
}