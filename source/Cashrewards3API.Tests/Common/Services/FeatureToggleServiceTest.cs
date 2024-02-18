using Cashrewards3API.Common;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Unleash;

namespace Cashrewards3API.Tests.Common.Services
{
    public class FeatureToggleServiceTest
    {
        private FeatureToggleService _featureToggle;
        private int? _premiumClientId = Constants.Clients.Blue;
        private Mock<IOptions<FeatureToggleOptions>> _iOptionsService;
        private Mock<IUnleash> _unleashMock;

        public FeatureToggleServiceTest()
        {
            _iOptionsService = new Mock<IOptions<FeatureToggleOptions>>();
        }

        [SetUp]
        public void Setup()
        {
            _unleashMock = new Mock<IUnleash>();
        }

        private FeatureToggleOptions featureToggleTrue = new FeatureToggleOptions() { Premium = true };
        private FeatureToggleOptions featureToggleFalse = new FeatureToggleOptions() { Premium = false };

        [Test]
        public void GetPremiumClientId_ShouldReturnNull_FromFeatureToggleTrueAndPremiumClientIdNull()
        {
            _iOptionsService.Setup(s => s.Value).Returns(featureToggleTrue);

            _featureToggle = new FeatureToggleService(_iOptionsService.Object, _unleashMock.Object);

            FeatureToggle featureToggle = _featureToggle.DisplayFeature(Enum.FeatureNameEnum.Premium, null);

            featureToggle.PremiumClientId.Should().BeNull();
            featureToggle.ShowFeature.Should().BeTrue();
        }

        [Test]
        public void GetPremiumClientId_ShouldReturnNull_FromFeatureToggleFalseAndPremiumClientId()
        {
            _iOptionsService.Setup(s => s.Value).Returns(featureToggleFalse);

            _featureToggle = new FeatureToggleService(_iOptionsService.Object, _unleashMock.Object);

            FeatureToggle featureToggle = _featureToggle.DisplayFeature(Enum.FeatureNameEnum.Premium, _premiumClientId);

            featureToggle.PremiumClientId.Should().BeNull();
            featureToggle.ShowFeature.Should().BeFalse();
        }

        [Test]
        public void GetPremiumClientId_ShouldBePremiumClientId_FromFeatureToggleTrueAndPremiumClientId()
        {
            _iOptionsService.Setup(s => s.Value).Returns(featureToggleTrue);

            _featureToggle = new FeatureToggleService(_iOptionsService.Object, _unleashMock.Object);

            FeatureToggle featureToggle = _featureToggle.DisplayFeature(Enum.FeatureNameEnum.Premium, _premiumClientId);

            featureToggle.PremiumClientId.Should().Be(_premiumClientId);
            featureToggle.ShowFeature.Should().BeTrue();
        }

        [Test]
        public void IsEnabled_UnleashApiCalled()
        {
            _featureToggle = new FeatureToggleService(_iOptionsService.Object, _unleashMock.Object);
            _unleashMock.Setup(p => p.IsEnabled(It.IsAny<string>())).Returns(true);
            var result =_featureToggle.IsEnabled("test");
            result.Should().BeTrue();
            _unleashMock.Verify(p => p.IsEnabled(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void IsEnabled_WhiteListEmail()
        {
            var featureService = new Mock<IFeatureToggle>();
            featureService.Setup(p => p.IsEnabled(It.IsAny<string>())).Returns(true);
            var result = featureService.Object.IsEnabled("test");
            result.Should().BeTrue();
        }
        [Test]
        public void IsNotEnabled_WhiteListEmail()
        {
            var featureService = new Mock<IFeatureToggle>();
            featureService.Setup(p => p.IsEnabled("unleashToggle")).Returns(true);
            var result = featureService.Object.IsEnabled("WrongToggle");
            result.Should().BeFalse();
        }
        
        [Test]
        public void Email_Should_Be_WhiteListEmail()
        {
            _featureToggle = new FeatureToggleService(_iOptionsService.Object, _unleashMock.Object);
            _unleashMock.Setup(p => p.IsEnabled(It.IsAny<string>())).Returns(true);
            var result = _featureToggle.IsEnabled("test");
            result.Should().BeTrue();
        }   
        
        [Test]
        public void Unleash_Toggled_Off_WhiteListEmail()
        {
            _featureToggle = new FeatureToggleService(_iOptionsService.Object, _unleashMock.Object);
            _unleashMock.Setup(p => p.IsEnabled(It.IsAny<string>())).Returns(false);
            var result = _featureToggle.IsEnabled("test");
            result.Should().BeFalse();
        }
    }
}