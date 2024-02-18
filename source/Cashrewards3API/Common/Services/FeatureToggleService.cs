using System.Collections.Generic;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Cashrewards3API.Options;
using Microsoft.Extensions.Options;
using Unleash;

namespace Cashrewards3API.Common.Services
{
    public class FeatureToggleService : IFeatureToggle
    {
        private readonly IOptions<FeatureToggleOptions> _featureToggleOptions;
        private IUnleash _unleashProxy { get; }

        public FeatureToggleService(IOptions<FeatureToggleOptions> featureToggle, IUnleash unleash)
        {
            _featureToggleOptions = featureToggle;
            _unleashProxy = unleash;
        }

        /// <summary>
        /// Displays the feature.
        /// </summary>
        /// <param name="featureEnum">The feature enum.</param>
        /// <param name="premiumClientId">The premium client identifier.</param>
        /// <returns></returns>
        public FeatureToggle DisplayFeature(FeatureNameEnum featureEnum, int? premiumClientId)
        {
            var featureToggle = new FeatureToggle() { PremiumClientId = premiumClientId, ShowFeature = false };

            return featureEnum switch
            {
                FeatureNameEnum.Premium => new FeatureToggle() { PremiumClientId = _featureToggleOptions.Value.Premium ? premiumClientId : null, ShowFeature = _featureToggleOptions.Value.Premium },
                _ => featureToggle,
            };
        }

        public bool IsEnabled(string toggleName)
        {
            return _unleashProxy.IsEnabled(toggleName);
        }
    }
}