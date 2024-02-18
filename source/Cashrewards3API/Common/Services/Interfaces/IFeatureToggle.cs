using Cashrewards3API.Common.Model;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Common.Services.Interfaces
{
    public interface IFeatureToggle
    {
        FeatureToggle DisplayFeature(FeatureNameEnum featureName,int? premiumClientId);

        bool IsEnabled(string toggleName);
    }
}