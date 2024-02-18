using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Person.Interface;
using Cashrewards3API.Features.Person.Model;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Common.Services
{
    public class PremiumServiceTests
    {
        private class TestState
        {
            public PremiumService PremiumService { get; }

            public Mock<IPerson> PersonService { get; }

            public TestState()
            {
                NotEnrolledPerson = new PersonModel
                {
                    CognitoId = Guid.NewGuid(),
                    PremiumStatus = Enum.PremiumStatusEnum.NotEnrolled
                };

                EnrolledPerson = new PersonModel
                {
                    CognitoId = Guid.NewGuid(),
                    PremiumStatus = Enum.PremiumStatusEnum.Enrolled,
                    Members = new List<Member> { EnrolledBlueMember }
                };

                OptOutPerson = new PersonModel
                {
                    CognitoId = Guid.NewGuid(),
                    PremiumStatus = Enum.PremiumStatusEnum.OptOut,
                    Members = new List<Member> { OptOutBlueMember }
                };

                PersonService = new Mock<IPerson>();
                PersonService.Setup(s => s.GetPerson(NotEnrolledPerson.CognitoId.ToString())).ReturnsAsync(NotEnrolledPerson);
                PersonService.Setup(s => s.GetPerson(EnrolledPerson.CognitoId.ToString())).ReturnsAsync(EnrolledPerson);
                PersonService.Setup(s => s.GetPerson(OptOutPerson.CognitoId.ToString())).ReturnsAsync(OptOutPerson);

                PremiumService = new PremiumService(PersonService.Object);
            }

            public PersonModel NotEnrolledPerson { get; } = new PersonModel { PremiumStatus = Enum.PremiumStatusEnum.NotEnrolled };

            public PersonModel EnrolledPerson { get; }
            public Member EnrolledBlueMember { get; } = new Member { MemberId = 111, ClientId = Constants.Clients.Blue };

            public PersonModel OptOutPerson { get; }
            public Member OptOutBlueMember { get; } = new Member { MemberId = 222, ClientId = Constants.Clients.Blue };
        }

        [Test]
        public async Task GetPremiumMembership_ShouldReturnNull_GivenNullOrEmptyCognitoId()
        {
            var state = new TestState();

            var premium = await state.PremiumService.GetPremiumMembership(Constants.Clients.CashRewards, string.Empty);

            premium.Should().BeNull();
            state.PersonService.Verify(p => p.GetPerson(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetPremiumMembership_ShouldReturnNull_GivenNoPersonExists()
        {
            var state = new TestState();

            var premium = await state.PremiumService.GetPremiumMembership(Constants.Clients.CashRewards, Guid.NewGuid().ToString());

            premium.Should().BeNull();
        }

        [Test]
        public async Task GetPremiumMembership_ShouldReturnNull_GivenPersonIsNotEnrolled()
        {
            var state = new TestState();

            var premium = await state.PremiumService.GetPremiumMembership(Constants.Clients.CashRewards, state.NotEnrolledPerson.CognitoId.ToString());

            premium.Should().BeNull();
        }

        [Test]
        public async Task GetPremiumMembership_ShouldReturnNull_GivenNonCashrewardsClientId()
        {
            var state = new TestState();

            var premium = await state.PremiumService.GetPremiumMembership(Constants.Clients.MoneyMe, state.EnrolledPerson.CognitoId.ToString());

            premium.Should().BeNull();
        }

        [Test]
        public async Task GetPremiumMembership_ShouldReturnPremiumMembership_GivenAnEnrolledPersion()
        {
            var state = new TestState();

            var premium = await state.PremiumService.GetPremiumMembership(Constants.Clients.CashRewards, state.EnrolledPerson.CognitoId.ToString());

            premium.Should().BeEquivalentTo(new PremiumMembership
            {
                PremiumClientId = Constants.Clients.Blue,
                PremiumMemberId = state.EnrolledBlueMember.MemberId,
                IsCurrentlyActive = true
            });
        }

        [Test]
        public async Task GetPremiumMembership_ShouldReturnInactivePremiumMembership_GivenAnOptOutPersion()
        {
            var state = new TestState();

            var premium = await state.PremiumService.GetPremiumMembership(Constants.Clients.CashRewards, state.OptOutPerson.CognitoId.ToString());

            premium.Should().BeEquivalentTo(new PremiumMembership
            {
                PremiumClientId = Constants.Clients.Blue,
                PremiumMemberId = state.OptOutBlueMember.MemberId,
                IsCurrentlyActive = false
            });
        }
    }
}
