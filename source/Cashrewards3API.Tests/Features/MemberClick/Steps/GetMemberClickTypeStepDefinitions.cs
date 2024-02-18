using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.MemberClick;
using Cashrewards3API.Features.MemberClick.Models;
using Cashrewards3API.Tests.Features.MemberClick.Utils;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;

namespace Cashrewards3API.Tests.Features.MemberClick.Steps
{
    [Binding]
    public class GetMemberClickTypeDetailsSteps
    {
        private readonly ScenarioContext scenarioContext;

        public MemberClickService MemberClickService { get; set; }
        public List<MerchantModelData> MerchantModelsTestData { get; set; }
        public List<OfferModelData> OfferModelsTestData { get; set; }
        public MemberClickTypeDetailsResultModel result { get; set; }
        public Exception exception { get; set; }

        public GetMemberClickTypeDetailsSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }

        [Given(@"a member makes a click")]
        public void GivenAMemberMakesAClick()
        {
            MerchantModelsTestData = TestDataLoader.Load<List<MerchantModelData>>(@"Features\MemberClick\JSON\MerchantViewModelResponse.json");
            OfferModelsTestData = TestDataLoader.Load<List<OfferModelData>>(@"Features\MemberClick\JSON\OfferModelResponse.json");
            var mockRepository = new Mock<IRepository>();
            mockRepository.Setup(s =>
                    s.Query<MerchantModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
                .ReturnsAsync((string query, object parameters, int? timeout) => MemberClickUtils.GetMerchantModelsTestData(MerchantModelsTestData, query, parameters));
            mockRepository.Setup(s =>
                s.Query<OfferModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
            .ReturnsAsync((string query, object parameters, int? timeout) => MemberClickUtils.GetOfferModelsTestData(OfferModelsTestData, query, parameters));
            MemberClickService = new MemberClickService(
                   mockRepository.Object,
                   null,
                   null,
                   null,
                   null,
                   null,
                   null,
                   null
               );
        }
        
        [When(@"memberclicktypehyphenatedstring is (.*)")]
        public async void WhenMemberclicktypehyphenatedstringIshyphenatedStringWithType(string hyphenatedStringWithType)
        {
            result = await MemberClickService.GetMemberClickTypeDetails(hyphenatedStringWithType, Constants.Clients.CashRewards);
        }

        [Then(@"the merchant hyphenated string is (.*) and merchantpaused is (.*)")]
        public void ThenTheMemberClickTypeDetailsAre(string merchantHyphenatedString, bool isMerchantPaused)
        {
            result.IsMerchantPaused.Should().Be(isMerchantPaused);
            result.MerchantHyphenatedString.Should().Be(merchantHyphenatedString);
        }

        [When(@"memberclicktypehyphenatedString is (.*)")]
        public async void WhenMemberclicktypehyphenatedStringIs(string invalidHyphenatedString)
        {
            try
            {
                result = await MemberClickService.GetMemberClickTypeDetails(invalidHyphenatedString, Constants.Clients.CashRewards);
            }
            catch (Exception ex)
            {
                this.exception = ex;
            }
        }           

        [Then(@"an exception is thrown")]
        public void ThenAnExceptionIsThrown()
        {
            exception.Should().NotBeNull();
        }


    }
}
