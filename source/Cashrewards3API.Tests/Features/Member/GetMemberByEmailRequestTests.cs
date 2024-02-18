using AutoMapper;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Member;
using Cashrewards3API.Features.Member.Interface;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Member.Request;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Member
{
    [TestFixture]
    public class GetMemberByEmailRequestTests
    {
        private class TestState
        {
            public Mock<IMemberService> memberServiceMock { get; }
            public Mock<IMapper> mapperMock { get; }
            public Mock<IFeatureToggle> featureServiceMock { get; }
            public Mock<IServiceProvider> requestServiceMock { get; }

            public TestState()
            {
                var expectMember = new EmailMemberDto()
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    MemberId = 1001112621,
                    MemberNewId = new Guid("c28c59b2-95b4-4c27-a5da-42453f94962e"),
                    AccessCode = "",
                    Email = "qa+Signup@cashre.com",
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
                memberServiceMock = new Mock<IMemberService>();
                memberServiceMock.Setup(s => s.GetMemberByEmail(It.Is<string>(s => s.Contains(expectMember.Email))))
                    .ReturnsAsync(new EmailMemberDto { IsValidated = true, Status=0});
                memberServiceMock.Setup(s => s.GetMemberByEmail(It.Is<string>(s => !s.Contains(expectMember.Email))))
                    .ReturnsAsync(new EmailMemberDto { IsValidated = true, Status = 1 });

                mapperMock = new Mock<IMapper>();

                featureServiceMock = new Mock<IFeatureToggle>();
                featureServiceMock.Setup(s => s.IsEnabled(It.IsAny<string>())).Returns(true);

                requestServiceMock = new Mock<IServiceProvider>();
                requestServiceMock
                    .Setup(x => x.GetService(typeof(IMapper)))
                    .Returns(mapperMock.Object);
            }
        }

        [TestCase("qa+SignupWithPromo@cashrewards.com", 1)]
        [TestCase("qa+signup+again@cashrewards.com", 1)]
        [TestCase("Tqa+Signup@cashrewards.com", 1)]
        [TestCase("qa+Signup@cashre.com", 0)]
        public async Task Get_Member_By_Email(string email, int status)
        {
            var state = new TestState();
            state.featureServiceMock.Setup(s => s.IsEnabled(It.IsAny<string>())).Returns(false);

            var memberController =
                new MemberInternalController(state.memberServiceMock.Object, state.mapperMock.Object, state.featureServiceMock.Object);
            memberController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = state.requestServiceMock.Object
            };
            var response = await memberController.GetMemberByEmail(email);
            var result = response as ObjectResult;
            state.memberServiceMock.Verify(s => s.GetMemberByEmail(It.IsAny<string>()), Times.Once);
            result!.StatusCode.Should().Be(200);
            result.Value.Should().BeOfType(typeof(EmailMemberDto));
            (result.Value as EmailMemberDto).Status.Should().Be(status);
        }
    }
}