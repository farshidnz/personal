using System;
using System.Collections.Generic;
using TimeZoneConverter;

namespace Cashrewards3API.Common
{
    public static class Constants
    {
        public static TimeZoneInfo SydneyTimezone => TZConvert.GetTimeZoneInfo("Australia/Sydney");


        public static class PolicyNames
        {
            public const string AllowAnonymousOrToken = "AllowAnonymousOrToken";

        }
        public static class Common
        {
            public const string HttpLink = "http://";
            public const string HttpsLink = "//";
        }

        public static class TrendingStores
        {
            public const string Mobile = "mobileapp";
            public const string Browser = "browser";
        }

        public static class PopularStores
        {
            public const string Mobile = "popular-MOBILE_APP";
            public const string Browser = "popular-BROWSER_APP";
        }

        public static class Channels
        {
            public const string InStoreChannelName = "In-Store";
            public const string OnlineChannelName = "Online";
        }

        public static class Commission
        {
            public const string Cashback = "cashback";
            public const string G29 = "G29";
            public const string Unknown = "???";
        }

        public static class RewardTypes
        {
            public const string Savings = "Savings";
            public const string Cashback = "Cashback";
        }

        public static class MerchantCommissionType
        {
            public const string Dollar = "dollar";
            public const string Percent = "percent";
        }

        public static class Clients
        {
            public const int CashRewards = 1000000;
            public const int MoneyMe = 1000033;
            public const int Blue = 1000034;
        }

        public static class Networks
        {
            #region NetworkIds

            public const int CommissionFactory = 1000008;

            public const int ChineseAN = 1000011;

            public const int DGMPerformance = 1000012;

            public const int PHAppleAustralia = 1000013;

            public const int PHAppleItunes = 1000014;

            public const int AffiliateWindow = 1000021;

            public const int PerformanceHorizon = 1000022;

            public const int Woolworths = 1000028;

            public const int ImpactRadius = 1000033;

            public const int ImpactRadiusLenovo = 1000036;

            public const int WilliamHillNetwork = 1000044;

            public const int ImpactRadiusAustralia = 1000051;

            public const int Bupa = 1000054;

            public const int TrueRewards = 1000063;

            #endregion NetworkIds

            #region network defines

            public static readonly HashSet<int> MobileSpecificNetworkIds = new HashSet<int>
            {
                1000061 // Button network
            };

            public static readonly HashSet<int> InStoreNetworkIds = new HashSet<int>
            {
                1000053 // In-store network
            };

            #endregion network defines
        }

        public class Merchants
        {
            public const int Woolworths = 1001330;
            public const int Masters = 1001846;
            public const int BigW = 1001847;
        }

        public static class BonusTransaction
        {
            public static TimeZoneInfo SydneyTimezone => TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
        }

        public static class RefferAFriend
        {
            public const string DefaultAccessCode = "referamate";
            public const string DefaultBonusType = "absolute";
            public const int PurchnaseWindowMax = 90;
            public const int SaleValueMin = 20;
        }

        public class MerchantReportingType
        {
            public const int TotalOrder = 101;
            public const int ItemLevel = 102;
            public const int SaleValueNotSupplied = 103;
        }

        public class Mapper
        {
            public const string ClientId = "ClientId";
            public const string TierTypePromotionId = "TierTypePromotionId";
            public const string CurrencyId = "CurrencyId";
            public const string Premium = "Premium";
            public const string TopicArn = "TopicArn";

            public const string PremiumStatus = "PremiumStatus";
            public const string DateTimeNow = "DateTimeNow";

            public const string SiteSlug = "SiteSlug";
            public const string Type = "Type";
            public const string EventCategory = "EventCategory";

            public const string CustomTrackingMerchantList = "CustomTrackingMerchantList";
            public const string HyphenatedString = "HyphenatedString";
            public const string MerchantId = "MerchantId";
            public const string DateTimeUTC = "DateTimeUTC";

            public const string Tiers = "Tiers";
        }

        public static readonly Dictionary<int, string> CommissionTypeDict = new Dictionary<int, string>()
        {
            [100] = "dollar",
            [101] = "percent"
        };

        public class BadgeCodes
        {
            public const string TripleCashback = "TripleCashback";
            public const string EndDateCount = "EndDateCount";
            public const string AnzPremiumOffers = "ANZPremiumOffers";
        }

        public class TrueRewards
        {
            public const string AuthTokenEndpoint = "/API/fetch-tr-widget-auth";
        }
    }
}