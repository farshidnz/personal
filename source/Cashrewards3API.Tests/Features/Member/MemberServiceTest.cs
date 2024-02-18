using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Services.Model;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Member.Repository;
using Cashrewards3API.Features.Member.Request.SignInMember;
using Cashrewards3API.Features.Member.Request.UpdateCognitoMember;
using Cashrewards3API.Features.Member.Service;
using Cashrewards3API.Features.Person.Interface;
using Cashrewards3API.Features.Person.Model;
using Cashrewards3API.Tests.Helpers;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Person
{
    [TestFixture]
    public class MemberServiceTest
    {
        private class TestState
        {
            public MemberService MemberService { get; }

            public Mock<IRepository> Repository { get; }

            public Mock<IDateTimeProvider> DateTimeProvider { get; }
            public Mock<ITokenService> Token { get; }
            public Mock<IMapper> Mapper { get; }
            public Mock<IEncryption> Cryptor { get; }
            public Mock<IPerson> PersonService { get; }
            public Mock<IMemberRepository> MemberRepository { get; }
            public Mock<ICacheKey> CacheKey { get; }
            public RedisUtilMock RedisUtilMock { get; }
            public Mock<CacheConfig> CacheConfig { get; }

            private TokenContext tokenMock = new TokenContext() { AuthToken = "theToken" };

            public TestState()
            {
                Repository = new Mock<IRepository>();
                Mapper = new Mock<IMapper>();
                Cryptor = new Mock<IEncryption>();
                PersonService = new Mock<IPerson>();
                MemberRepository = new Mock<IMemberRepository>();
                CacheKey = new Mock<ICacheKey>();
                RedisUtilMock = new RedisUtilMock().Setup<string>();
                CacheConfig = new Mock<CacheConfig>();
                Token = new Mock<ITokenService>();
                DateTimeProvider = new Mock<IDateTimeProvider>();
                var configs = new Dictionary<string, string>
                {
                    { "Config:TrueRewards:ApiKey", "F040B97A-A9BE-4439-8970-7E43AB94F5BA" },
                    { "Config:TrueRewards:App", "store"},
                    { "Config:TrueRewards:TokenIssuer", "https://truerewards.com.au"}
                };

                Repository.Setup(r => r.QueryFirstOrDefault<MemberModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(Member);
                Mapper.Setup(m => m.Map(It.IsAny<MemberDto>(), It.IsAny<MemberModel>())).Returns(Member);
                Mapper.Setup(m => m.Map<CognitoMemberModel>(It.IsAny<MemberModel>())).Returns(CognitoMember);
                Mapper.Setup(m => m.Map(It.IsAny<CognitoMemberModel>(), It.IsAny<MemberModel>())).Returns(Member);
                Mapper.Setup(m => m.Map<SignInMemberInternalResponseMember>(It.IsAny<MemberInternalModel>())).Returns(new SignInMemberInternalResponseMember());
                PersonService.Setup(p => p.GetPersonById(Person.PersonId)).ReturnsAsync(Person);
                MemberRepository.Setup(r => r.GetMemberModelByEmailAndClientId(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(Member);
                CacheKey.Setup(c => c.GetTRAuthTokenKey(It.IsAny<string>(), It.IsAny<string>())).Returns("TrueRewardsAuthToken:5_josh.kila@gmail.com");
                Token.Setup(token => token.GetToken(It.IsAny<AuthnRequestContext>())).ReturnsAsync(tokenMock);

                MemberService = new MemberService(Repository.Object, Repository.Object, Mapper.Object, Cryptor.Object,
                    PersonService.Object, MemberRepository.Object, CacheKey.Object, RedisUtilMock.Object, CacheConfig.Object, Token.Object, DateTimeProvider.Object);
            }

            public MemberModel Member { get; } = new MemberModel
            {
                MemberId = 111,
                PersonId = 222,
                ClientId = Constants.Clients.CashRewards,
                CognitoId = Guid.NewGuid(),
                FirstName = "FirstName",
                LastName = "LastName",
                MemberNewId = Guid.NewGuid(),
                AccessCode = "AccessCode",
                Email = "Email",
                DateJoined = DateTime.Now,
                OriginationSource = "Test",
                Mobile = "0400000000",
                CampaignId = 0,
                PostCode = "2067",
            };

            public MemberModel MemberANZ { get; } = new MemberModel
            {
                MemberId = 111,
                PersonId = 222,
                ClientId = Constants.Clients.Blue,
                CognitoId = Guid.NewGuid(),
                FirstName = "FirstName",
                LastName = "LastName",
                MemberNewId = Guid.NewGuid(),
                AccessCode = "AccessCode",
                Email = "Email",
                DateJoined = DateTime.Now,
                OriginationSource = "Test",
                Mobile = "0400000000",
                CampaignId = 0,
                PostCode = "2067",
            };

            public MemberModel MemberNewCognito { get; } = new MemberModel
            {
                MemberId = 111,
                PersonId = 222,
                ClientId = Constants.Clients.Blue,
                CognitoId = Guid.NewGuid(),
                FirstName = "FirstName",
                LastName = "LastName",
                MemberNewId = Guid.NewGuid(),
                AccessCode = "AccessCode",
                Email = "Email",
                DateJoined = DateTime.Now,
                OriginationSource = "Test",
                Mobile = "0400000000",
                CampaignId = 0,
                PostCode = "2067",
            };

            public PersonModel Person { get; } = new PersonModel { PersonId = 222, OriginationSource = "Test" };
            public CognitoMemberModel CognitoMember { get; } = new CognitoMemberModel { PersonId = 222, CognitoId = Guid.NewGuid().ToString() };
        }

        [Test]
        public void ValidateCreateCognitoMember_With_PersonCreated()
        {
            var state = new TestState();
            var member = state.MemberService.CreateCognitoMember(state.Member);
            member.Status.Should().Be(TaskStatus.RanToCompletion);
            member.Result.Should().BeOfType(typeof(MemberModel));
            member.Exception.Should().BeNull();

            state.Repository.Verify(x => x.ExecuteAsyncWithRetry(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(3));
        }
        [Test]
        public void ValidateCreateCognitoMember_WhiteListMember_With_PersonCreated()
        {
            var state = new TestState
            {
                Member =
                {
                    IsValidated = true
                }
            };
            var member = state.MemberService.CreateCognitoMember(state.Member);
            member.Status.Should().Be(TaskStatus.RanToCompletion);
            member.Result.Should().BeOfType(typeof(MemberModel));
            member.Exception.Should().BeNull();
            member.Result.IsValidated.Should().BeTrue();

            state.Repository.Verify(x => x.ExecuteAsyncWithRetry(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(3));
        }

        [Test]
        public void ReturnCognitoMember_With_Member_CognitoMember_And_Person_AlreadyExisted()
        {
            var state = new TestState();
            state.Repository.Setup(o => o.QueryAsync<MemberModel>(It.Is<string>(sql => sql.Contains("m.Email =")), It.IsAny<object>()))
                  .ReturnsAsync(new List<MemberModel>() { state.Member });
            var member = state.MemberService.CreateCognitoMember(state.Member);
          
            member.Result.Should().BeOfType(typeof(MemberModel));
            member.Exception.Should().BeNull();

            state.Repository.Verify(x => x.ExecuteAsyncWithRetry(It.IsAny<string>(), It.IsAny<object>()), Times.Never());
        }

        [Test]
        public void CreateCognitoMember_With_Person_AlreadyExisted_DifferentValues()
        {
            var state = new TestState();
            state.Repository.Setup(o => o.QueryAsync<MemberModel>(It.Is<string>(sql => sql.Contains("m.Email =")), It.IsAny<object>()))
                  .ReturnsAsync(new List<MemberModel>() { state.Member });
            var member = state.MemberService.CreateCognitoMember(state.MemberANZ);

            member.Result.Should().BeOfType(typeof(MemberModel));
            member.Exception.Should().BeNull();

            state.Repository.Verify(x => x.ExecuteAsyncWithRetry(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(4));
        }


        [Test]
        public void UpdateCognitoMember_With_Member_CognitoMember_And_Person_AlreadyExisted_DifferentValues()
        {
            var state = new TestState();
            state.Repository.Setup(o => o.QueryAsync<MemberModel>(It.Is<string>(sql => sql.Contains("m.Email =")), It.IsAny<object>()))
                  .ReturnsAsync(new List<MemberModel>() { state.Member });
            var member = state.MemberService.CreateCognitoMember(state.MemberNewCognito);

            member.Result.Should().BeOfType(typeof(MemberModel));
            member.Exception.Should().BeNull();

            state.Repository.Verify(x => x.ExecuteAsyncWithRetry(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(4));
        }


        [Test]
        public void UpdateFacebookUsername_ShouldThrowException_GivenDifferntEmailAddress()
        {
            var state = new TestState();
            var facebookUsernameRequest = new UpdateFacebookUsernameRequest
            {
                FacebookUsername = "NewEmail"
            };
            state.Invoking(async s => await s.MemberService.UpdateFacebookUsername(facebookUsernameRequest))
                .Should().Throw<NotFoundException>();
        }

        [Test]
        public async Task GetTRAuthToken_ShouldReturnAuthTokenFromTrueRewardsSite_WithCorrectParameters_WhenFirstAndLastNamePresent()
        {
            var state = new TestState();

            var member = new MemberContextModel();
            member.FirstName = "josh";
            member.LastName = "kila";
            member.Email = "josh.kila@gmail.com";

            var token = await state.MemberService.GetTRAuthToken(member);

            token.Should().Be("theToken");
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task GetTRAuthToken_ShouldReturnAuthTokenFromTrueRewardsSite_WithCorrectParameters_WhenFirstAndOrLastNameNotPresent(bool first, bool last)
        {
            var state = new TestState();

            var member = new MemberContextModel();
            member.FirstName = first ? "josh" : null;
            member.LastName = last ? "kila" : null;
            member.Email = "josh.kila@gmail.com";

            var token = await state.MemberService.GetTRAuthToken(member);
            token.Should().Be("theToken");
        }

        [TestCase("", "p@ssword")]
        [TestCase(null, "p@ssword")]
        [TestCase("some.one@somewhere.com", "")]
        [TestCase("some.one@somewhere.com", null)]
        public async Task SignInMember_ShouldReturnInvalidResponse_GivenEmailOrPasswordNotSupplied(string email, string password)
        {
            var state = new TestState();

            var response = await state.MemberService.SignInMember(new SignInMemberInternalRequest(email, password));

            response.Member.Should().BeNull();
            response.Status.Should().BeFalse();
            response.Msg.Should().Be("Email/Password required.");
        }

        [Test]
        public async Task SignInMember_ShouldReturnInvalidResponse_GivenUserDoesNotExist()
        {
            var state = new TestState();
            state.MemberRepository.Setup(r => r.GetMemberInternalModelByEmailAndClientId(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync((MemberInternalModel)null);

            var response = await state.MemberService.SignInMember(new SignInMemberInternalRequest("some.one@somewhere.com", "p@ssword"));

            response.Member.Should().BeNull();
            response.Status.Should().BeFalse();
            response.Msg.Should().Be("Member not found.");
        }

        [Test]
        public async Task SignInMember_ShouldReturnInvalidResponse_GivenUserPasswordIsIncorrect()
        {
            var state = new TestState();
            state.MemberRepository.Setup(r => r.GetMemberInternalModelByEmailAndClientId(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new MemberInternalModel());
            state.Cryptor.Setup(c => c.VerifyStringWithSalt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var response = await state.MemberService.SignInMember(new SignInMemberInternalRequest("some.one@somewhere.com", "p@ssword"));

            response.Member.Should().BeNull();
            response.Status.Should().BeFalse();
            response.Msg.Should().Be("Invalid credentials.");
        }

        [Test]
        public async Task SignInMember_ShouldReturnMember_GivenUserPasswordIsCorrect()
        {
            var state = new TestState();
            state.MemberRepository.Setup(r => r.GetMemberInternalModelByEmailAndClientId(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new MemberInternalModel());
            state.Cryptor.Setup(c => c.VerifyStringWithSalt(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var response = await state.MemberService.SignInMember(new SignInMemberInternalRequest("some.one@somewhere.com", "p@ssword"));

            response.Status.Should().BeTrue();
            response.Member.Should().NotBeNull();
        }

        [Test]
        public async Task MapMembersMemberNewIdToMemberNewIdWithClientId_ShouldReturnNewMemberId()
        {
            var state = new TestState();
            var memberNewIdGuid = Guid.NewGuid();
            var member = new Cashrewards3API.Features.Member.Model.Member() { MemberNewId = memberNewIdGuid };
            state.Repository.Setup(r => r.QueryFirstOrDefault<Cashrewards3API.Features.Member.Model.Member>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(member);
            var response = await state.MemberService.MapMembersMemberNewIdToMemberNewIdWithClientId("123-456", 100000);

            response.Should().NotBeNull();
            response.Should().Be(memberNewIdGuid);
        }
    }
}
