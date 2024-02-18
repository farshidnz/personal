using AutoMapper;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Member.Request.SignInMember;
using Cashrewards3API.Mapper;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using Cashrewards3API.Features.Member.Request;

namespace Cashrewards3API.Tests.Mapping
{
    [TestFixture]
    internal class MemberProfileMappingTest
    {
        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MemberProfile>());
            Mapper = config.CreateMapper();
        }

        private IMapper Mapper { get; set; }

        private readonly CognitoMemberModel _cognitoMemberModel = new()
        {
            CognitoId = "cafe00c8-9b54-4fbc-99e5-0d7a2ab2ec44",
            PersonId = 10000
        };
        private readonly CreateCognitoMemberRequest _cognitoWhiteListMemberModel = new()
        {
            Email = "test@cashrewards.com"
        };

        private readonly MemberInternalModel _memberInternalModel = new()
        {
            MemberNewId = Guid.Parse("beef002d-e454-1ebc-22e5-1234fa223aee"),
            MemberId = 12345,
            FirstName = "First",
            LastName = "Last",
            PostCode = "2069",
            AccessCode = "raf",
            Mobile = "0400 111 222",
            Status = 1
        };

        [Test]
        public void CanMap_CognitoMemberModel_To_MemberModel()
        {
            var member = new MemberModel();

            member = Mapper.Map(_cognitoMemberModel, member);

            member.CognitoId.Should().Be("cafe00c8-9b54-4fbc-99e5-0d7a2ab2ec44");
            member.PersonId.Should().Be(10000);
        }
       

        [Test]
        public void CanMap_MemberInternalModel_To_GetMemberInternalResponseMember()
        {
            var member = Mapper.Map<SignInMemberInternalResponseMember>(_memberInternalModel);

            member.MemberNewId.Should().Be("beef002d-e454-1ebc-22e5-1234fa223aee");
            member.MemberId.Should().Be(12345);
            member.FirstName.Should().Be("First");
            member.LastName.Should().Be("Last");
            member.PostCode.Should().Be("2069");
            member.AccessCode.Should().Be("raf");
            member.PhoneNumber.Should().Be("0400111222");
            member.Status.Should().Be(1);
        }
    }
}
