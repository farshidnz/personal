using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services.Model;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Person.Model;
using Cashrewards3API.Mapper;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Cashrewards3API.Tests.Mapping
{
    public class PersonProfileMappingTest
    {
        private IMapper mapper;
        private Person person;
        private PersonModel personModelBase;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<PersonProfile>());
            mapper = config.CreateMapper();

            person = new Person()
            {
                CognitoId = Guid.Parse("000AFD42-8BB8-4607-B1C0-1D4D53734F1B"),
                CreatedDateUTC = new DateTime(2021, 7, 20),
                Members = new List<Member>() { new Member() { ClientId = Constants.Clients.CashRewards, MemberNewId = Guid.Parse("000AFD42-8BB8-4607-B1C0-1D4D53734F1C"), MemberId = 1000000 } },
                OriginationSource = "originationSource",
                PersonId = 1000011,
                PremiumStatus = Enum.PremiumStatusEnum.OptOut,
                UpdatedDateUTC = null
            };

            personModelBase = new PersonModel()
            {
                PersonId = 1000011,
                PremiumStatus = Enum.PremiumStatusEnum.Enrolled
            };
        }

        [Test]
        public void PersonProfile_ShouldMapFieldsCorrectly()
        {
            PersonModel personModelTest = new PersonModel() { PremiumStatus =  Enum.PremiumStatusEnum.NotEnrolled};
             personModelTest =  mapper.Map<PersonModel>(person);

            personModelTest.Should().BeEquivalentTo(new PersonModel()
            {
                CashRewardsMemberId = 1000000,
                CognitoId = Guid.Parse("000AFD42-8BB8-4607-B1C0-1D4D53734F1B"),
                MemberNewId = "000AFD42-8BB8-4607-B1C0-1D4D53734F1C".ToLower(),
                Members = new List<Member>() { new Member() { ClientId = Constants.Clients.CashRewards, MemberNewId = Guid.Parse("000AFD42-8BB8-4607-B1C0-1D4D53734F1C"), MemberId = 1000000 } },
                OriginationSource = "originationSource",
                PersonId = 1000011,
                PremiumStatus = Enum.PremiumStatusEnum.OptOut
            });
        }

        [Test]
        public void PersonProfile_ShouldMapPersonPremiumHistory()
        {
            PersonPremiumStatusHistory premiumHistory = mapper.Map<PersonPremiumStatusHistory>(personModelBase, opts =>
            {
                opts.Items[Constants.Mapper.DateTimeUTC] = new DateTime(2021, 01, 20);
            });

            premiumHistory.Should().BeEquivalentTo(new PersonPremiumStatusHistory()
            {
                PersonId = 1000011,
                PremiumStatus = Enum.PremiumStatusEnum.Enrolled,
                ClientId = Constants.Clients.Blue,
                UpdatedDateUTC = new DateTime(2021, 01, 20),
                StartedAtUTC = new DateTime(2021, 01, 20),
                EndedAtUTC = new DateTime(2021, 01, 20),
                CreatedDateUTC = new DateTime(2021, 01, 20)
            });
        }
    }
}