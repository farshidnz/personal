using Cashrewards3API.Enum;
using Cashrewards3API.Features.Category;
using Cashrewards3API.Features.Merchant.Models;
using Microsoft.OpenApi.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cashrewards3API.Common.Services
{
    public interface ICacheKey
    {
        string GetRootCategoriesCacheKey(int clientId, Status status);

        string GetSubCategoriesCacheKey(int clientId, int rootCategoryId, Status status);

        string GetCardLinkedMerchantsKey(int clientId, int? premiumClientId, int categoryId);

        string GetFeaturedOffersKey(int clientId, int? premiumClientId, int categoryId, int offset, int limit,bool device);

        string GetFeaturedOffersForMobileKey(int clientId, int categoryId, int offset, int limit);

        string GetCashBackIncreasedOffersForMobileKey(int clientId, int? premiumClientId);

        string GetCashBackIncreasedOffersForBrowserKey(int clientId, int? premiumClientId);

        string GetCashOfferForClientsAndOfferIds(int clientId, int? premiumClientId, IEnumerable<int> offerIds);

        string GetMerchantBundleByMerchantIdKey(int clientId, int merchantId,int? premiumClientId,bool isMobile,IncludePremiumEnum includePremium = IncludePremiumEnum.Preview);
        
        string GetMerchantBundleByHyphenatedStringKey(int clientId, string hyphenatedString, int? premiumClientId, bool isMobile);

        string GetMerchantBundleByStoreKey(MerchantRequestInfoModel model);

        string GetPopularMerchantsForBrowserKey(int clientId, int? premiumClientId, int offset = 0, int limit = 12);

        string GetPopularMerchantsForMobileKey(int clientId, int? premiumClientId, int offset = 0, int limit = 12);

        string GetTrendingStoresForBrowserKey(int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12);

        string GetTrendingStoresForMobileKey(int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12);

        string GetAllStoresMerchantsKey(int clientId, int categoryId, int? premiumClientId, int offset = 0, int limit = 20);
        string GetTotalMerchantCountForClientKey(int clientId);
        string GetBannersForClientIds(int clientId);

        string GetSpecialOffersKey(int clientId, int? premiumClientId, OfferTypeEnum? offertype, bool isMobile);
        string GetGiftCardKey(int clientId, int? premiumClientId, string bucketKey);
        string GetPromotionKey(int clientId, int? premiumClientId, string slug);
        string GetCrApplicationKey(string key);
        string GetTRAuthTokenKey(string name, string email);
        string GetNetworkKey();
        string GetClientsKey();
        string GetMerchantFeedKey(int clientId, int? premiumClientId);
    }

    public class CacheKey : ICacheKey
    {
        private const string ClientIdText = "ClientId";
        private const string StatuText = "Status";
        private const string OffsetText = "Offset";
        private const string LimitText = "Limit";

        public string GetRootCategoriesCacheKey(int clientId, Status status)
        {
            return $"{ClientIdText}:{clientId}:RootCategories:{StatuText}:{status}";
        }

        public string GetSubCategoriesCacheKey(int clientId, int rootCategoryId, Status status)
        {
            return $"{ClientIdText}:{clientId}:RootCategories:{rootCategoryId}:Status:{status}";
        }

        public string GetCardLinkedMerchantsKey(int clientId, int? premiumClientId, int categoryId)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:ClieCardLinkedMerchants:CategoryId:{categoryId}";
        }

        public string GetFeaturedOffersForMobileKey(int clientId, int categoryId, int offset, int limit)
        {
            return $"{ClientIdText}:{clientId}:FeaturedOffersForMobile:CategoryId:{categoryId},{OffsetText}:{offset}:{LimitText}:{limit}";
        }

        public string GetFeaturedOffersKey(int clientId, int? premiumClientId, int categoryId, int offset, int limit,bool isMobile)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:FeaturedOffersForBrowser:CategoryId:{categoryId}:{OffsetText}:{offset}:{LimitText}:{limit}:{isMobile}";
        }

        public string GetCashBackIncreasedOffersForMobileKey(int clientId, int? premiumClientId)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:CashBackIncreasedOffersForMobile";
        }

        public string GetCashBackIncreasedOffersForBrowserKey(int clientId, int? premiumClientId)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:CashBackIncreasedOffersForBrowser";
        }

        public string GetCashOfferForClientsAndOfferIds(int clientId, int? premiumClientId, IEnumerable<int> offerIds)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:OfferIds:{string.Join(",", offerIds)}:GetCashOfferForClientsAndOfferIds";
        }

        public string GetMerchantBundleByMerchantIdKey(int clientId, int merchantId,int? premiumClientId, bool isMobile, IncludePremiumEnum includePremium = IncludePremiumEnum.Preview)
        {
            return $"{ClientIdText}:{clientId}:MerchantBundleByMerchantId:{merchantId}:{premiumClientId}:{isMobile}:{includePremium}";
        }

        public string GetMerchantBundleByHyphenatedStringKey(int clientId, string hyphenatedString, int? premiumClientId, bool isMobile)
        {
            return $"{ClientIdText}:{clientId}:MerchantBundleByHyphenatedString:{hyphenatedString}:{premiumClientId}:{isMobile}";
        }

        public string GetMerchantBundleByStoreKey(MerchantRequestInfoModel model)
        {
            return $"{ClientIdText}:{model.ClientId}:{model.PremiumClientId}:" +
                   $"MerchantBundleByStore:{model.CategoryId}:" +
                   $"{model.InStoreFlag.GetDisplayName()}:{model.Offset}:{model.Limit}";
        }

        public string GetPopularMerchantsForBrowserKey(int clientId, int? premiumClientId, int offset = 0, int limit = 12)
        {
            if (premiumClientId.HasValue)
                return $"{ClientIdText}:{premiumClientId}:{clientId}:PopularMerchantsForBrowser:{offset}:{limit}";

            return $"{ClientIdText}:{clientId}:PopularMerchantsForBrowser:{offset}:{limit}";
        }

        public string GetPopularMerchantsForMobileKey(int clientId, int? premiumClientId, int offset = 0, int limit = 12)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:PopularMerchantsForMobile:{offset}:{limit}";
        }

        public string GetTrendingStoresForBrowserKey(int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12)
        {
            if (premiumClientId.HasValue)
                return $"{ClientIdText}:{clientId}{premiumClientId}:TrendingStoresForBrowser:CategoryId:{categoryId}:{offset}:{limit}";

            return $"{ClientIdText}:{clientId}:TrendingStoresForBrowser:CategoryId:{categoryId}:{offset}:{limit}";
        }

        public string GetTrendingStoresForMobileKey(int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:TrendingStoresForMobile:CategoryId:{categoryId}:{offset}:{limit}";
        }

        public string GetAllStoresMerchantsKey(int clientId, int categoryId, int? premiumClientId,int offset = 0, int limit = 20)
        {
            return $"{ClientIdText}:{clientId}:AllStoresMerchants:CategoryId:{categoryId}:{offset}:{limit}:{premiumClientId}";
        }

        public string GetTotalMerchantCountForClientKey(int clientId)
        {
            return $"{ClientIdText}:{clientId}:GetTotalMerchantCountForClient";
        }

        public string GetBannersForClientIds(int clientId)
        {
            return $"{ClientIdText}:{clientId}:GetBanners";
        }

        public string GetGiftCardKey(int clientId, int? premiumClientId, string bucketKey)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:GiftCards:bucket{bucketKey}";
        }

        public string GetSpecialOffersKey(int clientId, int? premiumClientId, OfferTypeEnum? offertype, bool isMobile)
        {
            return $"{ClientIdText}:{clientId}:GetSpecialOffers:{premiumClientId}:{offertype}:{isMobile}";
        }

        public string GetPromotionKey(int clientId, int? premiumClientId, string slug)
        {
            return $"{ClientIdText}:{clientId}:{premiumClientId}:Promotion:slug{slug}";
        }

        public string GetCrApplicationKey(string key)
        {
            return $"SSMApplicationKey:{key}";
        }

        public string GetTRAuthTokenKey(string name, string email)
        {
            return $"TrueRewardsAuthToken:{name.Length}_{email}";
        }

        public string GetNetworkKey()
        {
            return $"NetworkKey:All";
        }

        public string GetClientsKey()
        {
            return $"ClientsKey:All";
        }

        public string GetMerchantFeedKey(int clientId, int? premiumClientId) => $"MerchantFeed:{clientId}:{premiumClientId}";
    }
}
