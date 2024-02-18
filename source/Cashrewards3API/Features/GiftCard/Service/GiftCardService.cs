using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Enum;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.GiftCard.Interface;
using Cashrewards3API.Features.GiftCard.Model;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.GiftCard.Service
{
    public class GiftCardService : IGiftCard
    {
        private readonly IConfiguration _configuration;
        private readonly IMerchantService _merchantService;
        private readonly IOfferService _offerService;
        private readonly ICacheKey _cacheKey;
        private readonly IMapper _mapper;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly IAwsS3Service _awsS3Service;

        public GiftCardService(IConfiguration configuration,
                                IMerchantService merchantService,
                                IOfferService offerService,
                                ICacheKey cacheKey,
                                IMapper mapper,
                                IRedisUtil redisUtil,
                                CacheConfig cacheConfig,
                                IAwsS3Service awsS3Service)
        {
            _configuration = configuration;
            _merchantService = merchantService;
            _offerService = offerService;
            _cacheKey = cacheKey;
            _mapper = mapper;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _awsS3Service = awsS3Service;
        }

        public async Task<Model.GiftCardDto> GetGiftCard(int clientId, int? premiumClientId, string bucketKey)
        {
            string key = _cacheKey.GetGiftCardKey(clientId, premiumClientId, bucketKey);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetGiftCardInfo(clientId, premiumClientId, bucketKey),
                _cacheConfig.MerchantDataExpiry);
        }

        private async Task<Model.GiftCardDto> GetGiftCardInfo(int clientId, int? premiumClientId, string bucketKey)
        {
            string giftCardDefinition = await GetGiftCardDefinition(bucketKey);

            if (string.IsNullOrEmpty(giftCardDefinition))
                throw new NotFoundException($"{bucketKey} giftcard not found");

            var giftCard = JsonConvert.DeserializeObject<Model.GiftCard>(giftCardDefinition,
                                    new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });

            IEnumerable<MerchantViewModel> merchants = await GetRequiredMerchants(clientId, premiumClientId, giftCard);
            IEnumerable<OfferDto> offers = await GetRequiredOffers(clientId, premiumClientId, giftCard);
            var giftCardInfo = _mapper.Map<GiftCardDto>(giftCard);

            giftCardInfo.Categories.ForEach(category => category.Items.ForEach(item =>
            {
                switch (item.ItemType)
                {
                    case (int)GiftCardCategoryItemTypeEnum.Merchant:
                    case (int)GiftCardCategoryItemTypeEnum.GiftCard2Go:
                    case (int)GiftCardCategoryItemTypeEnum.GiftCardWidget:
                        var merchant = merchants.FirstOrDefault(m => m.MerchantId == item.ItemId);
                        _mapper.Map<MerchantViewModel, GiftCardDto.CategoryDto.ItemDto>(merchant, item);
                        return;

                    case (int)GiftCardCategoryItemTypeEnum.Offer:
                    case (int)GiftCardCategoryItemTypeEnum.Offer2Go:
                        var offer = offers.FirstOrDefault(o => o.Id == item.ItemId);
                        _mapper.Map<OfferDto, GiftCardDto.CategoryDto.ItemDto>(offer, item);
                        return;
                }
            }));

            return giftCardInfo;
        }

        public virtual async Task<string> GetGiftCardDefinition(string bucketKey)
        {
            string bucket = _configuration["Config:GiftCardBucketName"];
            return await _awsS3Service.ReadAmazonS3Data($"{bucketKey}.json", bucket);
        }

        public async Task<IEnumerable<MerchantViewModel>> GetRequiredMerchants(int clientId, int? premiumClientId, Model.GiftCard giftCard)
        {
            var merchantIds = new HashSet<int>();
            giftCard.Categories?.ForEach(c => c.Items?.Where(p => p.ItemType == (int)GiftCardCategoryItemTypeEnum.Merchant 
                                                                  || p.ItemType == (int)GiftCardCategoryItemTypeEnum.GiftCard2Go
                                                                  || p.ItemType == (int)GiftCardCategoryItemTypeEnum.GiftCardWidget).ToList()
                .ForEach(merchant => merchantIds.Add(merchant.ItemId)));

            IEnumerable<MerchantViewModel> merchants;
            if (premiumClientId.HasValue)
                merchants = await _merchantService.GetMerchantsForStandardAndPremiumClients(clientId, premiumClientId.Value, merchantIds);
            else
                merchants = await _merchantService.GetMerchantsForStandardClient(clientId, merchantIds);

            return merchants;
        }

        public async Task<IEnumerable<OfferDto>> GetRequiredOffers(int clientId, int? premiumClientId, Model.GiftCard giftCard)
        {
            var offerIds = new HashSet<int>();
            giftCard.Categories?.ForEach(c => c.Items?.Where(p => p.ItemType == (int)GiftCardCategoryItemTypeEnum.Offer
                                                                  || p.ItemType == (int)GiftCardCategoryItemTypeEnum.Offer2Go).ToList()
                .ForEach(offer => offerIds.Add(offer.ItemId)));

            return await _offerService.GetClientOffers(clientId, premiumClientId, offerIds);
        }
    }
}