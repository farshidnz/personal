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
    public class CreateCognitoMemberRequestValidatorTests
    {
        private class TestState
        {
            public Mock<IMemberService> memberServiceMock { get; }
            public Mock<IMapper> mapperMock { get; }
            public Mock<IFeatureToggle> featureServiceMock { get; }
            public Mock<IServiceProvider> requestServiceMock { get; }

            public TestState()
            {
                memberServiceMock = new Mock<IMemberService>();
                memberServiceMock.Setup(s => s.CreateCognitoMember(It.IsAny<MemberModel>()))
                    .ReturnsAsync(new MemberModel { IsValidated = true });

                mapperMock = new Mock<IMapper>();
                mapperMock.Setup(
                        m => m.Map<MemberModel>(It.IsAny<CreateCognitoMemberRequest>()))
                    .Returns(new MemberModel() { IsValidated = false });

                featureServiceMock = new Mock<IFeatureToggle>();
                featureServiceMock.Setup(s => s.IsEnabled(It.IsAny<string>())).Returns(true);

                requestServiceMock = new Mock<IServiceProvider>();
                requestServiceMock
                    .Setup(x => x.GetService(typeof(IMapper)))
                    .Returns(mapperMock.Object);
            }
        }

        [Test]
        public void Validator_ShouldNotReturnValidationError_GivenEmailAndFacebookUserName_IgnoringCase_AreTheSame()
        {
            var validator = new CreateCognitoMemberRequestValidator();

            var model = new CreateCognitoMemberRequest
            {
                AccessCode = "123",
                CampaignId = 1,
                CognitoId = Guid.NewGuid(),
                FirstName = "test",
                LastName = "Last",
                Email = "test@test.com",
                FacebookUsername = "Test@test.com"
            };

            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(member => member.FacebookUsername);
        }

        [Test]
        public void Validator_ShouldReturnValidationError_GivenFacebookUserName_Empty()
        {
            var validator = new CreateCognitoMemberRequestValidator();

            var model = new CreateCognitoMemberRequest
            {
                AccessCode = "123",
                CampaignId = 1,
                CognitoId = Guid.NewGuid(),
                FirstName = "test",
                LastName = "Last",
                Email = "test@test.com"
            };

            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Validator_ShouldReturnValidationError_GivenEmailAndFacebookUserName_NotSame()
        {
            var validator = new CreateCognitoMemberRequestValidator();

            var model = new CreateCognitoMemberRequest
            {
                AccessCode = "123",
                CampaignId = 1,
                CognitoId = Guid.NewGuid(),
                FirstName = "test",
                LastName = "Last",
                Email = "test@test.com",
                FacebookUsername = "test2@test.com"
            };

            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(member => member.FacebookUsername);
        }

        [TestCase("qa+SignupWithPromo@cashrewards.com")]
        [TestCase("qa+Signupnopromo1234@cashrewards.com")]
        [TestCase("qa+signupworldcup1986@cashrewards.com")]
        [TestCase("Qa+Signupanz111143hey@cashrewards.com")]
        [TestCase("qa+Signupagain@cashrewards.COM")]
        [TestCase("qa+Signup@cashrewards.com")]
        [TestCase("qa+signup+001@cashrewards.com")]
        public async Task Create_WhiteList_Member(string email)
        {
            var state = new TestState();

            var memberController =
                new MemberInternalController(state.memberServiceMock.Object, state.mapperMock.Object, state.featureServiceMock.Object);
            memberController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = state.requestServiceMock.Object
            };
            var response = await memberController.RegisterCognitoMember(new CreateCognitoMemberRequest() { Email = email });
            var result = response as OkObjectResult;
            state.featureServiceMock.Verify(s => s.IsEnabled(It.IsAny<string>()), Times.Once);
            state.memberServiceMock.Verify(s => s.CreateCognitoMember(It.Is<MemberModel>(m => m.IsValidated == true)), Times.Once);
            result!.StatusCode.Should().Be(200);
        }

        [TestCase("qa+Signu@cashrewards.com")]
        [TestCase("qa+signu@cashrewards.com")]
        [TestCase("qa+SignupWithPromo@Cashreward.com")]
        [TestCase("Tqa+Signup@cashrewards.com")]
        public async Task Create_Non_WhiteList_Member(string email)
        {
            var state = new TestState();

            var memberController =
                new MemberInternalController(state.memberServiceMock.Object, state.mapperMock.Object, state.featureServiceMock.Object);
            memberController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = state.requestServiceMock.Object
            };
            var response = await memberController.RegisterCognitoMember(new CreateCognitoMemberRequest() { Email = email });
            var result = response as OkObjectResult;
            state.featureServiceMock.Verify(s => s.IsEnabled(It.IsAny<string>()), Times.Once);
            state.memberServiceMock.Verify(s => s.CreateCognitoMember(It.Is<MemberModel>(m => m.IsValidated == false)), Times.Once);
            result!.StatusCode.Should().Be(200);
        }

        [TestCase("qa+SignupWithPromo@cashrewards.com")]
        [TestCase("qa+signup+again@cashrewards.com")]
        [TestCase("Tqa+Signup@cashrewards.com")]
        [TestCase("qa+Signup@cashre.com")]
        public async Task Create_WhiteList_And_Non_WhiteList_Member_FeatureToggle_Off(string email)
        {
            var state = new TestState();
            state.featureServiceMock.Setup(s => s.IsEnabled(It.IsAny<string>())).Returns(false);

            var memberController =
                new MemberInternalController(state.memberServiceMock.Object, state.mapperMock.Object, state.featureServiceMock.Object);
            memberController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                RequestServices = state.requestServiceMock.Object
            };
            var response = await memberController.RegisterCognitoMember(new CreateCognitoMemberRequest() { Email = email });
            var result = response as ObjectResult;
            state.featureServiceMock.Verify(s => s.IsEnabled(It.IsAny<string>()), Times.Once);
            state.memberServiceMock.Verify(s => s.CreateCognitoMember(It.Is<MemberModel>(m => m.IsValidated == false)), Times.Once);
            result!.StatusCode.Should().Be(200);
        }
    }
}