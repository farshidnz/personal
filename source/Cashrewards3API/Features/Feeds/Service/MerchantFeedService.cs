using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Feeds.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Feeds.Service
{
    public interface IMerchantFeedService
    {
        Task<IEnumerable<MerchantFeedModel>> GetMerchantFeed(int clientId, int? premiumClientId);
    }

    public class MerchantFeedService : IMerchantFeedService
    {
        private readonly IRedisUtil _redisUtil;
        private readonly ICacheKey _cacheKey;
        private readonly CacheConfig _cacheConfig;
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly IMapper _mapper;
        private readonly List<int> _excludedMerchants = new();

        public MerchantFeedService(
            IRedisUtil redisUtil,
            ICacheKey cacheKey,
            CacheConfig cacheConfig,
            IReadOnlyRepository readOnlyRepository,
            IMapper mapper,
            IConfiguration configuration)
        {
            _redisUtil = redisUtil;
            _cacheKey = cacheKey;
            _cacheConfig = cacheConfig;
            _readOnlyRepository = readOnlyRepository;
            _mapper = mapper;
            if (int.TryParse(configuration["Config:Transaction:CashrewardsReferAMateMerchantId"], out var referAMateMerchant))
            {
                _excludedMerchants.Add(referAMateMerchant);
            }
        }

        public async Task<IEnumerable<MerchantFeedModel>> GetMerchantFeed(int clientId, int? premiumClientId) =>
            await _redisUtil.GetDataAsyncWithEarlyRefresh(
                _cacheKey.GetMerchantFeedKey(clientId, premiumClientId),
                () => GetMerchantFeedFromDb(clientId, premiumClientId),
                _cacheConfig.MerchantDataExpiry);

        private async Task<IEnumerable<MerchantFeedModel>> GetMerchantFeedFromDb(int clientId, int? premiumClientId)
        {
            var clientIds = new List<int> { clientId };
            if (premiumClientId.HasValue)
            {
                clientIds.Add(premiumClientId.Value);
            }

            var allTiers = (await GetMerchantTierFromDb(clientIds)).ToList();
            var allMerchants = (await GetMerchantFromDb(clientIds)).ToList();

            var tiersByMerchantId = allTiers
                .GroupBy(t => t.MerchantId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(g => g.ClientId).Aggregate(
                        new Dictionary<int, MerchantFeedTierModel>(),
                        (tiers, next) =>
                        {
                            if (next.ClientId == premiumClientId)
                            {
                                if (tiers.TryGetValue(next.MerchantTierId, out var t))
                                {
                                    var premium = _mapper.Map<MerchantFeedTierPremiumModel>(next);
                                    if (t.TierCashbackType != premium.TierCashbackType
                                        || t.TierCashback < premium.TierCashback)
                                    {
                                        t.Premium = premium;
                                    }
                                }
                                else
                                {
                                    var premiumOnlyTier = _mapper.Map<MerchantFeedTierModel>(next);
                                    tiers[next.MerchantTierId] = premiumOnlyTier;
                                    premiumOnlyTier.Premium = _mapper.Map<MerchantFeedTierPremiumModel>(next);
                                    premiumOnlyTier.TierCashback = 0;
                                }
                            }
                            else
                            {
                                tiers[next.MerchantTierId] = _mapper.Map<MerchantFeedTierModel>(next);
                            }

                            return tiers;
                        },
                        tiers => tiers.Values.ToList()
                    )
                );

            return allMerchants
                .GroupBy(m => m.MerchantId)
                .Select(m => _mapper.Map<MerchantFeedModel>(m.First(), opts =>
                {
                    opts.Items[Constants.Mapper.Tiers] = tiersByMerchantId.TryGetValue(m.First().MerchantId, out var t) ? t : new List<MerchantFeedTierModel>();
                }))
                .Where(m => m.MerchantTier != null && m.MerchantTier.Any())
                .ToList();
        }

        private async Task<IEnumerable<MerchantFeedDataModel>> GetMerchantFromDb(List<int> clientIds) =>
            await _readOnlyRepository.QueryAsync<MerchantFeedDataModel>(@"
                SELECT m.MerchantId,m.MerchantName,m.DescriptionShort,m.HyphenatedString,m.WebsiteUrl,mc.ClientId
                FROM Merchant m
                JOIN MerchantClientMap mc ON mc.MerchantId = m.MerchantId
                WHERE m.Status = 1
                  AND mc.ClientId IN @ClientIds
                  AND m.MerchantId NOT IN @ExcludedMerchants
                  AND m.MerchantId NOT IN (SELECT m.MerchantId from MaterialisedMerchantTierView mtv INNER JOIN MerchantMapping m ON mtv.MerchantId = m.MapMerchantId)",
                new
                {
                    ClientIds = clientIds,
                    ExcludedMerchants = _excludedMerchants
                });

        private async Task<IEnumerable<MerchantFeedTierDataModel>> GetMerchantTierFromDb(List<int> clientIds) =>
            await _readOnlyRepository.QueryAsync<MerchantFeedTierDataModel>(@"
                SELECT mt.MerchantId,mt.MerchantTierId,mt.Commission,mtc.ClientComm,mtc.MemberComm,mt.TierCommTypeId,mt.TierDescription,mtc.ClientId
                FROM MerchantTier mt
                JOIN MerchantTierClient mtc ON mtc.MerchantTierId = mt.MerchantTierId
                JOIN Client c ON c.ClientId = mtc.ClientId
                WHERE mt.Status = 1 AND mtc.Status = 1 AND c.Status = 1
                  AND (SYSUTCDATETIME() BETWEEN mtc.StartDateUtc AND mtc.EndDateUtc)
                  AND mtc.ClientId IN @ClientIds",
                new { ClientIds = clientIds });
    }
}
