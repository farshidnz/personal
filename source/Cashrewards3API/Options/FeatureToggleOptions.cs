namespace Cashrewards3API.Options
{
    public class FeatureToggleOptions
    {
        public bool Premium { get; set; }

        public UnleashConfig UnleashConfig { get; set; }
    }

    public class UnleashConfig
    {
        public string AppName { get; set; }

        public string UnleashApi { get; set; }

        public string Environment { get; set; }

        public int FetchTogglesIntervalMin { get; set; }

        public string UnleashApiKey { get; set; }
    }
}