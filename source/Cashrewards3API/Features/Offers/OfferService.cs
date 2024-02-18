using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Enum;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Offers
{
    public interface IOfferService
    {
        Task<PagedList<OfferDto>> GetFeaturedOffersAsync(
            int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 35, bool isMobileDevice = false);

        Task<PagedList<OfferDto>> GetFeaturedOffersForMobileAsync(
            int clientId, int categoryId, int offset = 0, int limit = 35);

        Task<IEnumerable<OfferDto>> GetCashBackIncreasedOffersForBrowser(int clientId, int? premiumClientId);

        Task<IEnumerable<OfferDto>> GetCashBackIncreasedOffersForMobile(int clientId, int? premiumClientId);

        Task<SpecialOffersDto> GetSpecialOffers(int clientId, int? premiumClientId, OfferTypeEnum? offertype, bool isMobile);

        Task<IEnumerable<OfferDto>> GetClientOffers(int clientId, int? premiumClientId, IEnumerable<int> offerIds);
    }

    public class OfferService : IOfferService
    {
        private readonly ILogger<OfferService> _logger;
        private readonly ICacheKey _cacheKey;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly INetworkExtension _networkExtension;
        private readonly string _customTrackingMerchantList;
        private readonly string _offerBackgroundImageDefault;
        private readonly IFeatureToggle _featureToggle;

        #region Constructor

        public OfferService(
            IConfiguration configuration,
            ILogger<OfferService> logger,
            ICacheKey cacheKey,
            IRedisUtil redisUtil,
            CacheConfig cacheConfig,
            IReadOnlyRepository readOnlyRepository,
            INetworkExtension networkExtension,
            IFeatureToggle featureToggle)
        {
            _logger = logger;
            _cacheKey = cacheKey;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _readOnlyRepository = readOnlyRepository;
            _networkExtension = networkExtension;
            _customTrackingMerchantList = configuration["Config:CustomTrackingMerchantList"];
            _offerBackgroundImageDefault = configuration["Config:OfferBackgroundImageDefault"];
            _featureToggle = featureToggle;
        }

        #endregion Constructor

        private const string OfferViewColumns = @"[OfferId],[ClientId],[MerchantId],[IsFeatured],[CouponCode],[OfferTitle],[AlteredOfferTitle],[MerchantTrackingLink],[OfferDescription],[TrackingLink],[HyphenatedString],[DateEnd],[CaptionCssClass],[Ranking],[OfferBackgroundImageUrl],[IsCashbackIncreased],[OfferBadgeCode],[MerchantName],[RegularImageUrl],[BasicTerms],[SmallImageUrl],[MediumImageUrl],[OfferCount],[TierCommTypeId],[TierTypeId],[Commission],[ClientComm],[MemberComm],[TierCssClass],[ExtentedTerms],[RewardName],[MerchantShortDescription],[MerchantMetaDescription],[MerchantHyphenatedString],[NetworkId],[TrackingTime],[ApprovalTime],[OfferTerms],[ClientProgramTypeId],[IsFlatRate],[MerchantBadgeCode],[OfferPastRate],[Rate],[NotificationMsg],[ConfirmationMsg],[IsToolbarEnabled],[TierCount],[RandomNumber],[IsCategoryFeatured],[IsPremiumFeature],[MerchantIsPremiumDisabled], [IsMerchantPaused]";

        public async Task<PagedList<OfferDto>> GetFeaturedOffersAsync(
           int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 35, bool isMobileDevice = false)
        {
            string key = _cacheKey.GetFeaturedOffersKey(clientId, premiumClientId, categoryId, offset, limit, isMobileDevice);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                () => GetFeaturedOffersFromDbAsync(clientId, premiumClientId, categoryId, offset, limit, isMobileDevice),
                                _cacheConfig.OfferDataExpiry);
        }

        private class OfferItem
        {
            public OfferViewModel BaseOffer { get; set; }
            public OfferMerchantModel BaseMerchant { get; set; }
            public OfferViewModel PremiumOffer { get; set; }
            public int Ranking => BaseOffer?.Ranking ?? PremiumOffer.Ranking;
            public DateTime DateEnd => BaseOffer?.DateEnd ?? PremiumOffer.DateEnd;
        }

        private async Task<PagedList<OfferDto>> GetFeaturedOffersFromDbAsync(int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 35, bool isMobileDevice = false)
        {
            var clientIds = new List<int> { clientId };
            if (premiumClientId.HasValue)
            {
                clientIds.Add(premiumClientId.Value);
            }
            var offers = await GetAllFeaturedOffers(clientIds, categoryId) ?? new List<OfferViewModel>();

            //TODO: Remove feature flag CPS-68
            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
            {
                offers = OfferHelpers.ExcludePausedMerchantOffers(offers);
            }

            if (isMobileDevice)
            {
                var mobileDisabledMerchantIds = await GetMobileDisabledMerchantIds(clientId);
                offers = ExcludeMobileDisabledMerchants(offers, mobileDisabledMerchantIds);
            }
            else
            {
                offers = ExcludeMobileSpecificNetworkIds(offers);
            }

            if (premiumClientId.HasValue)
            {
                offers = OfferHelpers.ExcludePremiumDisabledMerchantOffers(offers);
            }

            var offerDic = offers.GroupBy(p => p.OfferId)
                                 .ToDictionary(g => g.Key,
                                               g => new OfferItem
                                               {
                                                   BaseOffer = g.FirstOrDefault(o => o.ClientId == clientId),
                                                   PremiumOffer = g.FirstOrDefault(o => o.ClientId == premiumClientId)
                                               });

            // if existing offer valid for Premium but not valid for cr, try get merchant rate
            if (premiumClientId.HasValue &&
                offerDic.Values.ToList().Exists(p => p.BaseOffer == null && p.PremiumOffer != null))
            {
                await AddBaseMerchantForOffer(offerDic.Values.ToList(), clientId);
            }

            var orderedOffers = offerDic.Values
               .OrderByDescending(o => o.Ranking)
               .ThenBy(o => o.DateEnd)
               .ToList();

            var offerDtos = MapToOfferDto(orderedOffers).Where(o => o.Merchant.Commission > 0).ToList();
            var offersPagedData = GetOffersPagedData(offerDtos, offset, limit).ToList();
            offersPagedData.ForEach(o => o.OfferBadge = o.OfferBadge == Constants.BadgeCodes.AnzPremiumOffers ? "" : o.OfferBadge);
            return new PagedList<OfferDto>(orderedOffers.Count, offersPagedData.Count, offersPagedData);
        }

        public async Task<PagedList<OfferDto>> GetFeaturedOffersForMobileAsync(
            int clientId, int categoryId, int offset = 0, int limit = 35)
        {
            string key = _cacheKey.GetFeaturedOffersForMobileKey(clientId, categoryId, offset, limit);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetFeaturedOffersForMobileFromDbAsync(clientId, categoryId, offset, limit),
                _cacheConfig.OfferDataExpiry);
        }

        public async Task<PagedList<OfferDto>> GetFeaturedOffersForMobileFromDbAsync(
            int clientId, int categoryId, int offset = 0, int limit = 35)
        {
            var featuredOffers = await GetAllFeaturedOffers(new[] { clientId }, categoryId);
            var mobileDisabledMerchantIds = await GetMobileDisabledMerchantIds(clientId);
            var offers = ExcludeMobileDisabledMerchants(featuredOffers, mobileDisabledMerchantIds);

            var offerViewModels = offers as OfferViewModel[] ?? offers.ToArray();
            var offersPagedData = GetOffersPagedData(offerViewModels, offset, limit).Select(MapToOfferDto).ToList();
            return new PagedList<OfferDto>(offerViewModels.Count(), offersPagedData.Count, offersPagedData);
        }

        public async Task<IEnumerable<OfferDto>> GetClientOffers(int clientId, int? premiumClientId, IEnumerable<int> offersIds)
        {
            string key = _cacheKey.GetCashOfferForClientsAndOfferIds(clientId, premiumClientId, offersIds);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetClientOffersFromDbAsync(clientId, premiumClientId, offersIds),
                _cacheConfig.OfferDataExpiry);
        }

        private async Task<IEnumerable<OfferDto>> GetClientOffersFromDbAsync(int clientId, int? premiumClientId, IEnumerable<int> offersIds)
        {
            List<int?> clientIds = new List<int?> { clientId };
            if (premiumClientId.HasValue)
                clientIds.Add(premiumClientId);

            var offers = await GetClientOffers(clientIds.ToArray(), offersIds);
            offers = ExcludeMobileSpecificNetworkIds(offers);
            IEnumerable<OfferViewModel> premiumOffers = null;
            if (premiumClientId.HasValue)
                premiumOffers = offers.Where(o => o.ClientId == premiumClientId);
            offers = offers.Where(o => o.ClientId == clientId);

            Dictionary<int, OfferItem> offersById = MapPremiumOfferToOffer(offers, premiumOffers, premiumClientId);

            var orderedOffers = offersById.Values
                .OrderByDescending(o => o.Ranking)
                .ThenBy(o => o.DateEnd)
                .ToList();

            return MapToOfferDto(orderedOffers);
        }

        public async Task<IEnumerable<OfferDto>> GetCashBackIncreasedOffersForBrowser(int clientId, int? premiumClientId)
        {
            string key = _cacheKey.GetCashBackIncreasedOffersForBrowserKey(clientId, premiumClientId);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                () => GetCashBackIncreasedOffersForBrowserFromDbAsync(clientId, premiumClientId),
                                _cacheConfig.OfferDataExpiry);
        }

        private async Task<IEnumerable<OfferDto>> GetCashBackIncreasedOffersForBrowserFromDbAsync(int clientId, int? premiumClientId)
        {
            List<int?> clientIds = new List<int?> { clientId };
            if (premiumClientId.HasValue)
                clientIds.Add(premiumClientId);

            var offers = await GetCashBackIncreasedOffers(clientIds.ToArray());

            if (premiumClientId.HasValue)
            {
                offers = OfferHelpers.ExcludePremiumDisabledMerchantOffers(offers);
            }

            offers = ExcludeMobileSpecificNetworkIds(offers);
            IEnumerable<OfferViewModel> premiumOffers = null;
            if (premiumClientId.HasValue)
                premiumOffers = offers.Where(o => o.ClientId == premiumClientId);
            offers = offers.Where(o => o.ClientId == clientId);

            Dictionary<int, OfferItem> offersById = MapPremiumOfferToOffer(offers, premiumOffers, premiumClientId);

            var orderedOffers = offersById.Values
                .OrderByDescending(o => o.Ranking)
                .ThenBy(o => o.DateEnd)
                .ToList();
            var orderedOffersDto = MapToOfferDto(orderedOffers)?.Where(offer => offer.Merchant.Commission > 0).ToList();

            orderedOffersDto?.ForEach(o => o.OfferBadge = o.OfferBadge != Constants.BadgeCodes.AnzPremiumOffers ? o.OfferBadge : string.Empty);

            return orderedOffersDto;
        }

        /// <summary>
        /// Gets the special offers, CashBack Increased or Premium Feature Offers
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="premiumClientId">The premium client identifier.</param>
        /// <param name="offertype">The offertype. this is not required paramter in case you want only one type of offer</param>
        /// <param name="isMobile">Get offers for mobile.</param>
        /// <returns></returns>
        public async Task<SpecialOffersDto> GetSpecialOffers(int clientId, int? premiumClientId, OfferTypeEnum? offertype, bool isMobile)
        {
            string key = _cacheKey.GetSpecialOffersKey(clientId, premiumClientId, offertype, isMobile);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                () => GetSpecialOffersFromDbAsync(clientId, premiumClientId, offertype, isMobile),
                                _cacheConfig.OfferDataExpiry);
        }

        private async Task<SpecialOffersDto> GetSpecialOffersFromDbAsync(int clientId, int? premiumClientId, OfferTypeEnum? offerType, bool isMobile)
        {
            FeatureToggle featureToggle = _featureToggle.DisplayFeature(FeatureNameEnum.Premium, premiumClientId);

            var offers = await GetCashBackIncreasedAndPremiumOffers(clientId, premiumClientId, offerType, featureToggle.ShowFeature || premiumClientId.HasValue);

            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
                offers = OfferHelpers.ExcludePausedMerchantOffers(offers);
            
            if (isMobile)
            {
                var mobileDisabledMerchantIds = await GetMobileDisabledMerchantIds(clientId);
                offers = ExcludeMobileDisabledMerchants(offers, mobileDisabledMerchantIds);
            }
            else
            {
                offers = ExcludeMobileSpecificNetworkIds(offers);
            }

            if (premiumClientId.HasValue)
            {
                offers = OfferHelpers.ExcludePremiumDisabledMerchantOffers(offers);
            }

            offers = offers.Distinct(new OfferComparer()).ToList();

            IEnumerable<OfferViewModel> offersCashBackIncreased = null, offersPremiumFeatured = null;

            switch (offerType)
            {
                case OfferTypeEnum.CashbackIncreased: offersCashBackIncreased = offers.Where(o => o.IsCashbackIncreased); break;
                case OfferTypeEnum.PremiumFeature: offersPremiumFeatured = offers.Where(o => o.IsPremiumFeature); break;

                default:
                    offersCashBackIncreased = offers.Where(o => o.IsCashbackIncreased);
                    offersPremiumFeatured = offers.Where(o => o.IsPremiumFeature);
                    break;
            }

            Dictionary<int, OfferItem> offersCashBackIncreasedById = MapPremiumOfferToOffer(offersCashBackIncreased?.Where(o => o.ClientId == clientId && o.ClientCommission > 0), offersCashBackIncreased?.Where(o => o.ClientId == premiumClientId), premiumClientId);
            Dictionary<int, OfferItem> offersPremiumFeaturedById = MapPremiumOfferToOffer(offersPremiumFeatured?.Where(o => o.ClientId == clientId && o.ClientCommission > 0), offersPremiumFeatured?.Where(o => o.ClientId == Constants.Clients.Blue), Constants.Clients.Blue, featureToggle.ShowFeature);

            var orderedCashBackIncreasedOffers = offersCashBackIncreasedById?.Values
                .OrderByDescending(o => o.Ranking)
                .ThenBy(o => o.DateEnd)
                .ToList();

            //orderedCashBackIncreasedOffers = orderedCashBackIncreasedOffers.Where(o => o.BaseMerchant.ClientCommission > -1).ToList();
            if (premiumClientId.HasValue
                && orderedCashBackIncreasedOffers != null
                && orderedCashBackIncreasedOffers.Exists(p => p.BaseOffer == null && p.PremiumOffer != null))
            {
                await AddBaseMerchantForOffer(orderedCashBackIncreasedOffers, clientId);
            }
            var orderedCashBackIncreasedOffersDto = MapToOfferDto(orderedCashBackIncreasedOffers)?.ToList();
            orderedCashBackIncreasedOffersDto?.ForEach(o => o.OfferBadge = o.OfferBadge != Constants.BadgeCodes.AnzPremiumOffers ? o.OfferBadge : string.Empty);

            var orderedPremiumFeaturedOffers = offersPremiumFeaturedById?.Values
                .OrderByDescending(o => o.Ranking)
                .ThenBy(o => o.DateEnd)
                .ToList();
            if (orderedPremiumFeaturedOffers != null && orderedPremiumFeaturedOffers.Exists(p => p.BaseOffer == null && p.PremiumOffer != null))
            {
                await AddBaseMerchantForOffer(orderedPremiumFeaturedOffers, clientId);
            }
            var orderedPremiumFeaturedOffersDto = MapToOfferDto(orderedPremiumFeaturedOffers)?.ToList();
            orderedPremiumFeaturedOffersDto?.ForEach(o => o.OfferBadge = o.OfferBadge == Constants.BadgeCodes.AnzPremiumOffers ? o.OfferBadge : string.Empty);

            return new SpecialOffersDto
            {
                CashBackIncreasedOffers = orderedCashBackIncreasedOffersDto,
                PremiumFeatureOffers = orderedPremiumFeaturedOffersDto
            };
        }

        public async Task<IEnumerable<OfferDto>> GetCashBackIncreasedOffersForMobile(int clientId, int? premiumClientId)
        {
            string key = _cacheKey.GetCashBackIncreasedOffersForMobileKey(clientId, premiumClientId);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                () => GetCashBackIncreasedOffersForMobileFromDbAsync(clientId, premiumClientId),
                                _cacheConfig.OfferDataExpiry);
        }

        /// <summary>
        /// Maps the offer to premium offer if applicable.
        /// </summary>
        /// <param name="offers">The offers.</param>
        /// <param name="premiumClientId">The premium client identifier.</param>
        /// <returns></returns>
        private Dictionary<int, OfferItem> MapPremiumOfferToOffer(IEnumerable<OfferViewModel> offers, IEnumerable<OfferViewModel> premiumOffers, int? premiumClientId, bool isPremiumFeature = false)
        {
            Dictionary<int, OfferItem> offersById = offers?.Where(o => (bool)isPremiumFeature ? premiumOffers.Any(of => of.OfferId == o.OfferId) : true).ToDictionary(o => o.OfferId, o => new OfferItem { BaseOffer = o });

            if (premiumClientId.HasValue && offersById != null)
            {
                foreach (var premiumOffer in premiumOffers)
                {
                    if (offersById.TryGetValue(premiumOffer.OfferId, out var offer))
                    {
                        offer.PremiumOffer = premiumOffer;
                    }
                    else
                    {
                        offersById.Add(premiumOffer.OfferId, new OfferItem { PremiumOffer = premiumOffer });
                    }
                }
            }
            return offersById;
        }

        private async Task<IEnumerable<OfferDto>> GetCashBackIncreasedOffersForMobileFromDbAsync(int clientId, int? premiumClientId, int offset = 0, int limit = 35)
        {
            var clientIds = new List<int?> { clientId };
            if (premiumClientId.HasValue)
                clientIds.Add(premiumClientId);

            var cashBackIncreasedoffers = await GetCashBackIncreasedOffers(clientIds.ToArray());
            var mobileDisabledMerchantIds = await GetMobileDisabledMerchantIds(clientId);
            var offers = ExcludeMobileDisabledMerchants(cashBackIncreasedoffers, mobileDisabledMerchantIds);

            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
                offers = OfferHelpers.ExcludePausedMerchantOffers(offers);

            if (premiumClientId.HasValue)
            {
                offers = OfferHelpers.ExcludePremiumDisabledMerchantOffers(offers);
            }

            IEnumerable<OfferViewModel> premiumOffers = null;
            if (premiumClientId.HasValue)
                premiumOffers = offers.Where(o => o.ClientId == premiumClientId);
            offers = offers.Where(o => o.ClientId == clientId);

            Dictionary<int, OfferItem> offersById = MapPremiumOfferToOffer(offers, premiumOffers, premiumClientId);

            var orderedOffers = offersById.Values
                .OrderByDescending(o => o.Ranking)
                .ThenBy(o => o.DateEnd)
                .ToList();

            if (orderedOffers.Exists(p => p.BaseOffer == null && p.PremiumOffer != null))
            {
                await AddBaseMerchantForOffer(orderedOffers, clientId);
            }
            var offerDtos = MapToOfferDto(orderedOffers).Where(offer => offer.Merchant.Commission > 0);
            var orderedOffersDto = GetOffersPagedData(offerDtos, offset, limit).ToList();
            orderedOffersDto?.ForEach(o => o.OfferBadge = o.OfferBadge != Constants.BadgeCodes.AnzPremiumOffers ? o.OfferBadge : string.Empty);

            return orderedOffersDto;
        }

        private IEnumerable<T> GetOffersPagedData<T>(IEnumerable<T> offers, int offset = 0, int limit = 35)
        {
            if (offset > 0)
            {
                offers = offers.Skip(offset).ToList();
            }

            if (limit > 0)
            {
                offers = offers.Take(limit).ToList();
            }

            return offers;
        }

        private async Task<IEnumerable<OfferViewModel>> GetAllFeaturedOffers(IEnumerable<int> clientIds, int categoryId)
        {
            IEnumerable<OfferViewModel> hotOffers;
            if (categoryId > 0)
            {
                hotOffers = (await GetAllCategoryFeaturedOffersAsync(clientIds))
                    .OrderByDescending(o => o.Ranking)
                    .ThenBy(o => o.DateEnd)
                    .ToList();

                var categoriedOffers = await GetOffersByCategoryIdAsync(clientIds, categoryId);

                hotOffers = hotOffers
                    .Where(p => categoriedOffers
                    .Exists(cOffer => p.OfferId == cOffer.OfferId))
                    .ToList();
            }
            else
            {
                hotOffers = (await GetAllHotOffersAsync(clientIds))
                    .OrderByDescending(o => o.Ranking)
                    .ThenBy(o => o.DateEnd)
                    .ToList();
            }

            return hotOffers;
        }

        private async Task<IEnumerable<OfferViewModel>> GetAllCategoryFeaturedOffersAsync(IEnumerable<int> clientIds)
        {
            string sql = $@"SELECT {OfferViewColumns} FROM MaterialisedOfferView
                                 WHERE ClientId IN @ClientIds AND IsCategoryFeatured = 1";

            return await _readOnlyRepository.Query<OfferViewModel>(sql, new
            {
                ClientIds = clientIds
            });
        }

        public async Task<List<OfferViewModel>> GetOffersByCategoryIdAsync(IEnumerable<int> clientIds, int categoryId)
        {
            const string SQLQuery = @"SELECT Offer.* FROM MaterialisedOfferView Offer
                                      INNER JOIN MerchantCategoryMap MerchCategory on Offer.MerchantId = MerchCategory.MerchantId
                                      WHERE MerchCategory.CategoryId = @CategoryId AND offer.ClientId IN @ClientIds";

            var OffersByCategory = await _readOnlyRepository.Query<OfferViewModel>(SQLQuery, new
            {
                ClientIds = clientIds,
                CategoryId = categoryId
            });

            return OffersByCategory.ToList();
        }

        private async Task<List<OfferViewModel>> GetAllHotOffersAsync(IEnumerable<int> clientIds)
        {
            string sql = $@"SELECT {OfferViewColumns} FROM MaterialisedOfferView
                                 WHERE ClientId IN @clientIds AND IsFeatured = 1";

            return await _readOnlyRepository.Query<OfferViewModel>(sql, new
            {
                ClientIds = clientIds
            });
        }

        private async Task<IEnumerable<OfferViewModel>> GetCashBackIncreasedOffers(int?[] clientIds)
        {
            string SQLQuery = $@"SELECT {OfferViewColumns} FROM MaterialisedOfferView
                                      WHERE ClientId  IN @ClientId AND IsCashbackIncreased = 1";

            var OffersByCategory = await _readOnlyRepository.Query<OfferViewModel>(SQLQuery,
                    new
                    {
                        ClientId = clientIds
                    }
                   );

            return OffersByCategory
                .OrderByDescending(offer => offer.Ranking)
                .ThenBy(offer => offer.DateEnd)
                .ToList();
        }

        private async Task<IEnumerable<OfferViewModel>> GetClientOffers(int?[] clientIds, IEnumerable<int> offersIds)
        {
            string SQLQuery = $@"SELECT {OfferViewColumns} FROM MaterialisedOfferView
                                      WHERE ClientId  IN @ClientId AND OfferId IN @OfferId";

            var OffersByCategory = await _readOnlyRepository.Query<OfferViewModel>(SQLQuery,
                new
                {
                    ClientId = clientIds,
                    OfferId = offersIds
                }
            );

            return OffersByCategory
                .OrderByDescending(offer => offer.Ranking)
                .ThenBy(offer => offer.DateEnd)
                .ToList();
        }

        private async Task<IEnumerable<OfferViewModel>> GetCashBackIncreasedAndPremiumOffers(int clientId, int? premiumClientId, OfferTypeEnum? offerType, bool premiumFeature = true)
        {
            List<int?> ClientIdsForPremiumFeature = new List<int?>() { clientId };
            List<int?> ClientIdsForCashBackIncreased = new List<int?>() { clientId };

            if (premiumClientId.HasValue)
                ClientIdsForCashBackIncreased.Add(premiumClientId);

            string SQLQuery = string.Empty;

            string sqlIsCashBackIncrease = $@"SELECT {OfferViewColumns} FROM MaterialisedOfferView WITH (NOLOCK) WHERE ClientId  IN @ClientIdsForCashBackIncreased AND IsCashbackIncreased = 1";
            string sqlIsPremiumFeature = $@" SELECT {OfferViewColumns} FROM MaterialisedOfferView WITH (NOLOCK)
                                      WHERE ClientId in @ClientIdsForPremiumFeature AND IsPremiumFeature = 1";

            if (premiumFeature)
            {
                ClientIdsForPremiumFeature.Add(Constants.Clients.Blue);
            }

            switch (offerType)
            {
                case null: SQLQuery = string.Concat(sqlIsCashBackIncrease, " UNION ", sqlIsPremiumFeature); break;

                case OfferTypeEnum.CashbackIncreased: SQLQuery = sqlIsCashBackIncrease; break;
                case OfferTypeEnum.PremiumFeature: SQLQuery = sqlIsPremiumFeature; break;

                default:
                    SQLQuery = string.Concat(sqlIsCashBackIncrease, " UNION ", sqlIsPremiumFeature); break;
            }

            var SpecialOffers = await _readOnlyRepository.Query<OfferViewModel>(SQLQuery,
                    new
                    {
                        ClientIdsForCashBackIncreased = ClientIdsForCashBackIncreased.ToArray(),
                        ClientIdsForPremiumFeature = ClientIdsForPremiumFeature.ToArray()
                    }, 50
                   );

            return SpecialOffers;
        }

        private IEnumerable<OfferViewModel> ExcludeMobileSpecificNetworkIds(IEnumerable<OfferViewModel> offers)
        {
            return offers
                    .Where(
                        offer => !_networkExtension.IsInMobileSpecificNetwork(offer.NetworkId)
                     ).ToList();
        }

        private IEnumerable<OfferViewModel> ExcludeMobileDisabledMerchants(IEnumerable<OfferViewModel> offers, List<int> mobileDisabledMerchantIds)
        {
            return offers
                    .Where(
                        offer => !mobileDisabledMerchantIds.Contains(offer.MerchantId)
                     ).ToList();
        }

        private async Task<List<int>> GetMobileDisabledMerchantIds(int clientId)
        {
            var mobileDisabledMerchants = await _readOnlyRepository.GetAllAsync<MobileDisabledMerchnt>();
            var mobileDisabledMerchantIds = mobileDisabledMerchants
                .Where(m => m.ClientId == clientId && !(m.IsMobileAppEnabled ?? true))
                .Select((m => m.MerchantId))
                .ToList();

            return mobileDisabledMerchantIds;
        }

        private IEnumerable<OfferDto> MapToOfferDto(IEnumerable<OfferItem> featuredOffers)
        {
            return featuredOffers?.Select(o =>
            {
                if (o.BaseOffer != null)
                {
                    var offer = MapToOfferDto(o.BaseOffer);
                    if (o.PremiumOffer != null)
                    {
                        offer.Premium = GetPremiumModel(o.PremiumOffer);
                    }
                    return offer;
                }
                else
                {
                    var offer = MapToOfferDto(o.PremiumOffer);
                    offer.Premium = GetPremiumModel(o.PremiumOffer);

                    if (o.BaseOffer == null && o.BaseMerchant != null)
                    {
                        offer.ClientCommissionString = o.BaseMerchant.ClientCommissionString;
                        offer.Merchant.Commission = o.BaseMerchant.ClientCommission;
                        offer.Merchant.CommissionType = GetMerchantCommissionTypeStringCore(o.BaseMerchant.TierCommTypeId);
                        offer.Merchant.IsFlatRate = o.BaseMerchant.IsFlatRate ?? false;
                        offer.Merchant.RewardType = GetRewardTypeString(o.BaseMerchant.TierTypeId);
                    }
                    return offer;
                }
            });
        }

        private OfferDto MapToOfferDto(OfferViewModel offer)
        {
            return new OfferDto
            {
                Id = offer.OfferId,
                Title = offer.OfferTitle,
                CouponCode = offer.CouponCode,
                EndDateTime = offer.DateEnd,
                Description = offer.OfferDescription,
                HyphenatedString = offer.HyphenatedString,
                IsFeatured = offer.IsFeatured,
                Terms = offer.OfferTerms,
                MerchantId = offer.MerchantId,
                MerchantLogoUrl = offer.RegularImageUrl,
                Merchant = GetMerchantBasicModel(offer),
                OfferBackgroundImageUrl = string.IsNullOrWhiteSpace(offer.OfferBackgroundImageUrl) ?
                                           _offerBackgroundImageDefault : offer.OfferBackgroundImageUrl,
                OfferBadge = string.IsNullOrWhiteSpace(offer.OfferBadgeCode) ?
                                        string.Empty : offer.OfferBadgeCode,
                IsCashbackIncreased = offer.IsCashbackIncreased,
                WasRate = string.IsNullOrWhiteSpace(offer.OfferPastRate) ? null : offer.OfferPastRate,
                ClientCommissionString = offer.ClientCommissionString,
                IsPremiumFeature = offer.IsPremiumFeature,
                RegularImageUrl = offer.RegularImageUrl,
                MerchantHyphenatedString = offer.MerchantHyphenatedString,
                IsMerchantPaused = offer.IsMerchantPaused
            };
        }

        private MerchantBasicModel GetMerchantBasicModel(OfferViewModel viewOffer)
        {
            return new MerchantBasicModel()
            {
                Id = viewOffer.MerchantId,
                Name = viewOffer.MerchantName,
                HyphenatedString = viewOffer.MerchantHyphenatedString,
                LogoUrl = viewOffer.RegularImageUrl,
                Description = viewOffer.MerchantShortDescription,
                Commission = viewOffer.ClientCommission,
                CommissionType = GetMerchantCommissionTypeStringCore(viewOffer.TierCommTypeId),
                IsFlatRate = viewOffer.IsFlatRate ?? false,
                OfferCount = viewOffer.OfferCount,
                RewardType = GetRewardTypeString(viewOffer.TierTypeId),
                IsCustomTracking = IsCustomTrackingCore(viewOffer.MerchantId),
                IsPaused = viewOffer.IsMerchantPaused,
                MerchantBadge = string.IsNullOrWhiteSpace(viewOffer.MerchantBadgeCode) ?
                                string.Empty : viewOffer.MerchantBadgeCode
            };
        }

        private Premium GetPremiumModel(OfferViewModel viewOffer)
        {
            return new Premium
            {
                Commission = viewOffer.ClientCommission,
                IsFlatRate = viewOffer.IsFlatRate ?? false,
                ClientCommissionString = viewOffer.ClientCommissionString
            };
        }

        private string GetMerchantCommissionTypeStringCore(int tierCommTypeId)
        {
            switch (tierCommTypeId)
            {
                case 100:
                    return Constants.MerchantCommissionType.Dollar;

                case 101:
                    return Constants.MerchantCommissionType.Percent;

                default:
                    return string.Empty;
            }
        }

        private string GetRewardTypeString(int TierTypeId)
        {
            if (TierTypeId == 121 || TierTypeId == 117)
            {
                return Constants.RewardTypes.Savings;
            }
            return Constants.RewardTypes.Cashback;
        }

        private bool IsCustomTrackingCore(int merchantId)
        {
            var merchantList = _customTrackingMerchantList
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => System.Convert.ToInt32(n))
                .ToArray();

            return merchantList.Contains(merchantId);
        }

        private async Task<IEnumerable<OfferMerchantModel>> GetMerchantsById(IEnumerable<int> merchantIds, int clientId)
        {
            string sql = $@"SELECT [MerchantId],
                                   [TierCommTypeId],
                                   [TierTypeId],
                                   [Commission],
                                   [ClientComm],
                                   [MemberComm],
                                   [Rate],
                                   [ClientProgramTypeId],
                                   [IsFlatRate],
                                   [RewardName] 
                            FROM [MaterialisedMerchantFullView]
                            WHERE [MerchantId] IN @MerchantIds AND ClientId = @ClientId";

            return await _readOnlyRepository.Query<OfferMerchantModel>(sql, new
            {
                MerchantIds = merchantIds,
                ClientId = clientId
            });
        }

        private async Task AddBaseMerchantForOffer(IEnumerable<OfferItem> offerList, int baseClientId)
        {
            var merchantIds = offerList
                                      .Where(p => p.BaseOffer == null && p.PremiumOffer != null)
                                      .Select(p => p.PremiumOffer.MerchantId).ToList();
            var merchantRates = await GetMerchantsById(merchantIds, baseClientId);

            foreach (var offer in offerList.Where(p => p.BaseOffer == null && p.PremiumOffer != null))
            {
                offer.BaseMerchant = merchantRates.FirstOrDefault(p => p.MerchantId == offer.PremiumOffer.MerchantId);
            }
        }
    }
}