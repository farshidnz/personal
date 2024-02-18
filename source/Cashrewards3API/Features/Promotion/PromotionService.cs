using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Features.Promotion.Model;
using Cashrewards3API.FeaturesToggle;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Promotion
{
    public interface IPromotionService
    {
        Task<PromotionDto> GetPromotionInfo(int clientId, int? premiumClientId, string slug);
    }

    public class PromotionService : IPromotionService
    {
        private readonly IPromotionDefinitionService _promotionDefinitionService;
        private readonly IMerchantService _merchantService;
        private readonly IOfferService _offerService;
        private readonly IMapper _mapper;
        private readonly IFeatureToggle _featureToggle;
        private readonly IPausedMerchantFeatureToggle _pausedMerchantFeatureToggle;

        public PromotionService(
            IPromotionDefinitionService promotionDefinitionService,
            IMerchantService merchantService,
            IOfferService offerService,
            IMapper mapper,
            IFeatureToggle featureToggle,
            IPausedMerchantFeatureToggle pausedMerchantFeatureToggle)
        {
            _promotionDefinitionService = promotionDefinitionService;
            _merchantService = merchantService;
            _offerService = offerService;
            _mapper = mapper;
            _featureToggle = featureToggle;
            _pausedMerchantFeatureToggle = pausedMerchantFeatureToggle;
        }


        public async Task<PromotionDto> GetPromotionInfo(int clientId, int? premiumClientId, string slug)
        {
            var promotionDefinition = await _promotionDefinitionService.GetPromotionDefinition(slug);

            var disabledMerchantIds = premiumClientId.HasValue
                ? (await _merchantService.GetPremiumDisabledMerchants())
                    .Select(m => m.MerchantId)
                    .ToHashSet()
                : new HashSet<int>();

            var merchants = await GetPromotionMerchants(clientId, premiumClientId, promotionDefinition, disabledMerchantIds);
            
            var offers = await GetPromotionOffers(clientId, premiumClientId, promotionDefinition, disabledMerchantIds);

            merchants = _pausedMerchantFeatureToggle.ExcludeItemsWhenIsPaused<MerchantViewModel>(merchants);
            offers = _pausedMerchantFeatureToggle.ExcludeItemsWhenIsMerchantPaused<OfferDto>(offers);

            var promotionDto = _mapper.Map<PromotionDto>(promotionDefinition);

            if (promotionDefinition?.MainMerchantList != null)
                promotionDto.MainMerchantList = _mapper.Map<List<PromotionMerchantInfo>>(promotionDefinition.MainMerchantList);

            promotionDto?.Categories.ForEach(category =>
            {
                category = _pausedMerchantFeatureToggle.ExcludeCategoryItemsWhenIsPaused(category, merchants, offers);

                category.Merchants?.ForEach(m =>
                {
                    var merchant = merchants.FirstOrDefault(mv =>
                        mv.MerchantId == m.MerchantId);
                    _mapper.Map<MerchantViewModel, PromotionMerchantData>(merchant, m);
                });

                category.Items.ForEach(item =>
                {
                    switch ((PromotionCategoryItemTypeEnum)item.ItemType)
                    {
                        case PromotionCategoryItemTypeEnum.Merchant:
                        case PromotionCategoryItemTypeEnum.Merchant2Go:
                        {
                            var merchant = merchants.FirstOrDefault(m => m.MerchantId == item.ItemId);
                            _mapper.Map<MerchantViewModel, PromotionCategoryItemData>(merchant, item);
                            break;
                        }
                        case PromotionCategoryItemTypeEnum.Offer:
                        {
                            var offer = offers.FirstOrDefault(o => o.Id == item.ItemId);
                            _mapper.Map<OfferDto, PromotionCategoryItemData>(offer, item);
                            break;
                        }
                        case PromotionCategoryItemTypeEnum.Offer2Go:
                        {
                            var offer = offers.FirstOrDefault(o => o.Id == item.ItemId);
                            _mapper.Map<OfferDto, PromotionCategoryItemData>(offer, item);
                            item.MerchantHyphenatedString = null;
                            break;
                        }
                        default:
                            break;
                    }
                });
            });

            var campaignSection = promotionDefinition.CampaignSection;
            promotionDto.CampaignSection.HeadImageUrls = _mapper.Map<HeadImageUrls>(campaignSection);
            var offerIds = campaignSection.Campaigns
                                    ?.SelectMany(p => p.Offers, (sectionDef, offerDef) =>
                                        offerDef.OfferId)
                                    .ToList();

            var validatedOffers = await _offerService.GetClientOffers(clientId, premiumClientId, offerIds ?? new List<int>());
            var campaigns = promotionDefinition.CampaignSection.Campaigns?.Select(p =>
                GetCampaign(p, validatedOffers.ToList()));

            promotionDto.CampaignSection.Campaigns = campaigns?.ToList();
            return promotionDto;
        }

        private CampaignInfo GetCampaign(CampaignDefinition campaignDef, List<OfferDto> validateOffers)
        {
            var campaignInfo = _mapper.Map<CampaignInfo>(campaignDef);

            campaignInfo.Offers = campaignDef.Offers
                ?.Where(p => validateOffers.Exists(validate => validate.Id == p.OfferId))
                .Select(p => GetOffer(p, validateOffers.First(validate => validate.Id == p.OfferId))).ToList();
            return campaignInfo;
        }

        private OfferInfo GetOffer(OfferDefinition offerDef, OfferDto offer)
        {
            var offerInfo = _mapper.Map<OfferInfo>(offerDef, opts =>
            {
                opts.Items[Constants.Mapper.HyphenatedString] = offer.HyphenatedString;
                opts.Items[Constants.Mapper.MerchantId] = offer.MerchantId;
            });
            return offerInfo;
        }

        private static bool IsMerchantTypeItem(PromotionCategoryItemTypeEnum itemType) =>
            itemType == PromotionCategoryItemTypeEnum.Merchant || itemType == PromotionCategoryItemTypeEnum.Merchant2Go;

        private static bool IsOfferTypeItem(PromotionCategoryItemTypeEnum itemType) =>
            itemType == PromotionCategoryItemTypeEnum.Offer || itemType == PromotionCategoryItemTypeEnum.Offer2Go;

        public async Task<IEnumerable<MerchantViewModel>> GetPromotionMerchants(int clientId, int? premiumClientId, PromotionDefinition promotion, HashSet<int> disabledMerchantIds)
        {
            var merchantIds = new HashSet<int>();
            promotion.Categories?.ForEach(c =>
                merchantIds.UnionWith(c.Items?.Where(i => IsMerchantTypeItem(i.ItemType) && !disabledMerchantIds.Contains(i.ItemId)).Select(i => i.ItemId)));

            var merchants = premiumClientId.HasValue
                ? await _merchantService.GetMerchantsForStandardAndPremiumClients(clientId, premiumClientId.Value, merchantIds)
                : await _merchantService.GetMerchantsForStandardClient(clientId, merchantIds);

            if (_featureToggle.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED))
                return MerchantHelpers.ExcludePausedMerchants<MerchantViewModel>(merchants);

            return merchants;
        }

        public async Task<IEnumerable<OfferDto>> GetPromotionOffers(int clientId, int? premiumClientId, PromotionDefinition promotion, HashSet<int> disabledMerchantIds)
        {
            var offerIds = new HashSet<int>();
            promotion.Categories?.ForEach(c =>
                offerIds.UnionWith(c.Items?.Where(i => IsOfferTypeItem(i.ItemType)).Select(i => i.ItemId)));

            var offers = await _offerService.GetClientOffers(clientId, premiumClientId, offerIds);

            offers = offers.Where(o => !disabledMerchantIds.Contains(o.MerchantId));

            if (_featureToggle.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED))
                return OfferHelpers.ExcludePausedMerchantOffers<OfferDto>(offers);

            return offers;
        }
    }
}
