using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Common.Utils.Extensions;
using Cashrewards3API.Features.Merchant;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Cashrewards3API.Features.CardLinkedMerchant
{
    public interface ICardLinkedMerchantService
    {
        Task<List<CardLinkedMerchantDto>> GetCardLinkedMerchantsAsync(int clientId, int? premiumClientId,
            int categoryId);
    }

    public class CardLinkedMerchantService : ICardLinkedMerchantService
    {
        private readonly int MerchantTierCommandTypeId;

        private readonly ICacheKey _cacheKey;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public CardLinkedMerchantService(
            IConfiguration configuration,
            ICacheKey cacheKey,
            IRedisUtil redisUtil,
            CacheConfig cacheConfig,
            IReadOnlyRepository readOnlyRepository)
        {
            _cacheKey = cacheKey;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _readOnlyRepository = readOnlyRepository;
            if (!string.IsNullOrEmpty(configuration["Config:MerchantTierCommandTypeId"]))
            {
                MerchantTierCommandTypeId = Convert.ToInt32(configuration["Config:MerchantTierCommandTypeId"]);
            }
        }

        public async Task<List<CardLinkedMerchantDto>> GetCardLinkedMerchantsAsync(int clientId, int? premiumClientId,
            int categoryId)
        {
            string key = _cacheKey.GetCardLinkedMerchantsKey(clientId, premiumClientId, categoryId);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetCardLinkedMerchantsFromDbAsync(clientId, premiumClientId, categoryId),
                _cacheConfig.CardLinkedMerchantDataExpiry);
        }

        private async Task<List<CardLinkedMerchantDto>> GetCardLinkedMerchantsFromDbAsync(int clientId,
            int? premiumClientId, int categoryId)
        {
            var cardLinkedMerchants = (await GetCardLinkedMerchantsByClientIdAsync(clientId, premiumClientId))
                                       .Where(merchant => merchant.Commission > 0).ToList();

            if (categoryId > 0 && cardLinkedMerchants.IsAny())
            {
                var merchantIdsByCategory = await GetMerchantIdsByCategoryAsync(clientId, categoryId);

                cardLinkedMerchants = cardLinkedMerchants
                    .Where(p => merchantIdsByCategory.Contains(p.MerchantId))
                    .ToList();
            }

            return cardLinkedMerchants;
        }

        private async Task<List<CardLinkedMerchantDto>> GetCardLinkedMerchantsByClientIdAsync(int clientId,
            int? premiumClientId)
        {
            var carLinkedMerchants = new List<CardLinkedMerchantDto>();

            var cardLinkedMerchants = await GetCardLinkedMerchantsFromDbAsync(clientId, premiumClientId);

            if (premiumClientId.HasValue)
            {
                cardLinkedMerchants = MerchantHelpers.ExcludePremiumDisabledMerchants(cardLinkedMerchants);
            }

            if (cardLinkedMerchants.IsAny())
            {
                var merchantGroups = cardLinkedMerchants.GroupBy(m => m.MerchantHyphenatedString);

                if (merchantGroups.IsAny())
                {
                    var merchants = await GetMerchantFullViewByIdsAsync(
                        cardLinkedMerchants.Select(p => p.MerchantId).ToArray());

                    var formattedLinkedMerchants =
                        FormatMerchantGroupInfo(merchantGroups, merchants, clientId, premiumClientId);

                    carLinkedMerchants = formattedLinkedMerchants
                        .OrderByDescending(r => r.IsHomePageFeatured)
                        .ThenByDescending(r => r.IsFeatured)
                        .ThenByDescending(r => r.IsPopular)
                        .ThenBy(r => r.MerchantName)
                        .ToList();
                }
            }

            return carLinkedMerchants;
        }

        private async Task<IEnumerable<CardLinkedMerchantViewModel>> GetCardLinkedMerchantsFromDbAsync(int clientId,
            int? premiumClientId)
        {
            var clientIds = new List<int> {clientId};
            if (premiumClientId.HasValue)
            {
                clientIds.Add(premiumClientId.Value);
            }

            const string sql =
                @"SELECT       MFV.MerchantName, MFV.HyphenatedString AS MerchantHyphenatedString, CLN.CardIssuer, MTV.TierReference AS OfferId, MTV.EndDate, MTV.TierSpecialTerms AS CardLinkedSpecialTerms, 
                                                         MTV.TierImageUrl AS BackgroundImageUrl, MFV.RegularImageUrl AS LogoImageUrl, CLN.InStore, MFV.Commission * MFV.ClientComm / 100.0 * MFV.MemberComm / 100.0 AS Commission, MFV.TierCommTypeId, 
                                                         MFV.MerchantId, MFV.ClientId, MFV.IsFeatured, MFV.IsHomePageFeatured, MFV.IsPopular,
						                                 MFV.IsFlatRate, MFV.TierCommTypeId, MFV.IsPremiumDisabled
                                FROM            dbo.MaterialisedMerchantFullView AS MFV INNER JOIN
                                                         dbo.CardLinkedNetwork AS CLN ON MFV.NetworkId = CLN.CardLinkedNetworkId INNER JOIN
                                                         dbo.MaterialisedMerchantTierView AS MTV ON MTV.MerchantId = MFV.MerchantId
                                WHERE MFV.ClientId IN @ClientIds";


            return await _readOnlyRepository.Query<CardLinkedMerchantViewModel>(sql, new
            {
                ClientIds = clientIds
            });
        }

        private async Task<IEnumerable<CardLinkedMerchantFullViewModel>> GetMerchantFullViewByIdsAsync(
            IEnumerable<int> merchantIds)
        {
            const string sql = @"SELECT * FROM MaterialisedMerchantFullView
                                 WHERE MerchantId in @MerchantIds
                                 ORDER BY MerchantName ";

            return await _readOnlyRepository.Query<CardLinkedMerchantFullViewModel>(sql, new
            {
                MerchantIds = merchantIds
            });
        }

        private async Task<IList<int>> GetMerchantIdsByCategoryAsync(int clientId, int categoryId)
        {
            const string sql = @"SELECT merchant.MerchantId AS MerchantId FROM MaterialisedMerchantFullView merchant 
                                 INNER JOIN MerchantCategoryMap merchantCategory  
                                 ON merchant.MerchantId = merchantCategory.MerchantId
                                 WHERE merchant.ClientId = @ClientId AND merchantCategory.CategoryId = @CategoryId ";

            var merchantIds = await _readOnlyRepository.Query<CardLinkedMerchant>(sql, new
            {
                ClientId = clientId,
                CategoryId = categoryId
            });

            return merchantIds.Select(m => m.MerchantId).ToList();
        }

        private IEnumerable<CardLinkedMerchantDto> FormatMerchantGroupInfo(
            IEnumerable<IGrouping<string, CardLinkedMerchantViewModel>> merchantGroups,
            IEnumerable<CardLinkedMerchantFullViewModel> merchantInfoList,
            int clientId,
            int? premiumClientId)
        {
            var formattedLinkedMerchants = new List<CardLinkedMerchantDto>();
            foreach (var merchantGroup in merchantGroups)
            {
                var standardMerchant = merchantGroup.FirstOrDefault(m => m.ClientId == clientId);
                var premiumMerchant = premiumClientId.HasValue
                    ? merchantGroup.FirstOrDefault(m => m.ClientId == premiumClientId)
                    : null;
                var merchant = standardMerchant ?? premiumMerchant;

                if (merchant.Commission != null)
                {
                    var cardLinkedMerchant = new CardLinkedMerchantDto()
                    {
                        MerchantHyphenatedString = merchant.MerchantHyphenatedString,
                        MerchantId = merchant.MerchantId,
                        BackgroundImageUrl = merchant.BackgroundImageUrl?
                            .Replace(Constants.Common.HttpLink, Constants.Common.HttpsLink),
                        CardIssuer = merchant.CardIssuer,
                        ClientId = merchant.ClientId,
                        CardLinkedSpecialTerms = merchant.CardLinkedSpecialTerms,
                        EndDate = merchant.EndDate,
                        LogoImageUrl = merchant.LogoImageUrl,
                        MerchantName = merchant.MerchantName,
                        OfferId = merchant.OfferId,
                        IsFeatured = merchant.IsFeatured,
                        IsHomePageFeatured = merchant.IsHomePageFeatured,
                        IsPopular = merchant.IsPopular,
                        MerchantBadge = merchantInfoList
                            .FirstOrDefault(p => p.MerchantId == merchant.MerchantId)
                            ?.MerchantBadgeCode,
                        Commission = Math.Round((decimal) merchant.Commission.Value, 2),
                        IsFlatRate = merchant.IsFlatRate,
                        CommissionType = Constants.CommissionTypeDict[merchant.TierCommTypeId],
                        Premium = premiumMerchant == null
                            ? null
                            : new PremiumCardLinkedMerchant
                            {
                                CommissionString = GetCommissionString(premiumMerchant.Commission,
                                    premiumMerchant.TierCommTypeId),
                                Commission = Math.Round((decimal)premiumMerchant.Commission.Value, 2),
                                IsFlatRate = premiumMerchant.IsFlatRate,
                                CommissionType = Constants.CommissionTypeDict[premiumMerchant.TierCommTypeId],
                            }
                    };

                    if (merchantGroup.Any(group => @group.InStore == true))
                    {
                        cardLinkedMerchant.Channels.Add(Constants.Channels.InStoreChannelName);
                    }

                    if (merchantGroup.Any(group => @group.InStore == false))
                    {
                        cardLinkedMerchant.Channels.Add(Constants.Channels.OnlineChannelName);
                    }

                    cardLinkedMerchant.CommissionString = GetCommissionString(merchant.Commission, merchant.TierCommTypeId);

                    formattedLinkedMerchants.Add(cardLinkedMerchant);
                }
            }

            return formattedLinkedMerchants;
        }

        private string GetCommissionString(decimal? merchantCommission, int merchantTierCommTypeId)
        {
            var commission = merchantCommission.HasValue
                ? (merchantTierCommTypeId == MerchantTierCommandTypeId
                    ? $"{Math.Round(merchantCommission.Value, 2).ToString(Constants.Commission.G29)}%"
                    : $"${Math.Round(merchantCommission.Value).ToString(Constants.Commission.G29)}")
                : $"{Constants.Commission.Unknown}";

            return $"{commission} {Constants.Commission.Cashback}";
        }
    }
}

