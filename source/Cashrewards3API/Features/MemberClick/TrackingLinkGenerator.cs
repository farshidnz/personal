using System.Web;

namespace Cashrewards3API.Features.MemberClick
{
    public class TrackingLinkGenerator
    {
        public string GenerateTrackingLinkByNetwork(string trackingLinkTemplate, NetworkModel network, string trackingRef, int memberId, int clientId)
        {
            switch (network.NetworkId)
            {
                // https://shopgoau.atlassian.net/wiki/display/DEV/Link+Structure+-+Performance+Horizon
                // Performance Horizon Deep Link rule:
                case Common.Constants.Networks.PHAppleAustralia:
                case Common.Constants.Networks.PHAppleItunes:
                case Common.Constants.Networks.PerformanceHorizon:
                    {
                        if (trackingLinkTemplate.IndexOf("/destination") != -1)
                        {
                            var index = trackingLinkTemplate.IndexOf("/destination");
                            return trackingLinkTemplate.Insert(index, network.TrackingHolder + trackingRef);
                        }

                        break;
                    }

                // https://shopgoau.atlassian.net/wiki/display/DEV/Commission+Factory+-+Link+Structure+Generation
                case Common.Constants.Networks.CommissionFactory:
                    {
                        break;
                    }

                // As per Dane's  JIRA case for DGM network//Added merchantid for lenovo
                case Common.Constants.Networks.DGMPerformance:
                case Common.Constants.Networks.ImpactRadiusLenovo:
                case Common.Constants.Networks.ImpactRadius:
                case Common.Constants.Networks.ImpactRadiusAustralia:
                    {
                        if (trackingLinkTemplate.IndexOf(network.TrackingHolder) != -1)
                        {
                            var index = trackingLinkTemplate.IndexOf(network.TrackingHolder) + network.TrackingHolder.Length;
                            return trackingLinkTemplate.Insert(index, trackingRef);
                        }
                        else if (trackingLinkTemplate.IndexOf(network.DeepLinkHolder) != -1)
                        {
                            var index = trackingLinkTemplate.IndexOf(network.DeepLinkHolder);
                            var trackingHolder = network.TrackingHolder.StartsWith("?") ? network.TrackingHolder.Substring(1) : network.TrackingHolder;
                            return trackingLinkTemplate.Insert(index, trackingHolder + trackingRef + "&");
                        }

                        break;
                    }

                //DWS-163
                case Common.Constants.Networks.AffiliateWindow:
                    {
                        if (trackingLinkTemplate.IndexOf(network.TrackingHolder) != -1)
                        {
                            var index = trackingLinkTemplate.IndexOf(network.TrackingHolder) + network.TrackingHolder.Length;
                            return trackingLinkTemplate.Insert(index, trackingRef);
                        }
                        else if (trackingLinkTemplate.IndexOf("&p") != -1)
                        {
                            var index = trackingLinkTemplate.IndexOf("&p");
                            return trackingLinkTemplate.Insert(index, network.TrackingHolder + trackingRef);
                        }

                        break;
                    }

                case Common.Constants.Networks.WilliamHillNetwork:
                    {
                        // William Hill Network -- We need to strip the '-1000000' so that the link becomes '1000112570' (the memberid component only)
                        return trackingLinkTemplate + network.TrackingHolder + memberId;
                    }

                case Common.Constants.Networks.Woolworths:
                    {
                        return trackingLinkTemplate + network.TrackingHolder + $"{memberId}-{clientId}";
                    }

                default:
                    {
                        break;
                    }
            }

            return trackingLinkTemplate + network.TrackingHolder + trackingRef;
        }

        public string GenerateTrackingLinkForAliasByNetwork(string trackingLinkTemplate, NetworkModel network, string trackingRef, string aliasDeeplink)
        {
            var trackingUrl = $"{trackingLinkTemplate}{network.TrackingHolder}{trackingRef}{network.DeepLinkHolder}{HttpUtility.UrlEncode(aliasDeeplink)}";

            return trackingUrl;
        }
    }
}
