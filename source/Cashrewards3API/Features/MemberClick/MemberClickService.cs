using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Events;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils.Extensions;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.MemberClick.Models;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.MemberClick
{

    public interface IMemberClickService
    {
        Task<TrackingLinkResultModel> GetMemberClickWithTrackingUrlAsync(TrackingLinkInfoModel trackingInfo);
        Task<MemberClickTypeDetailsResultModel> GetMemberClickTypeDetails(string hyphenatedStringWithType, int clientId);
    }

    public class MemberClickService : IMemberClickService
    {
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly TrackingLinkGenerator _trackingLinkGenerator;
        private readonly IdGeneratorFactory _idGeneratorFactory;
        private readonly IWoolworthsEncryptionProvider _woolworthsEncryptionProvider;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly CommonConfig _commonConfig;
        private readonly IMapper _mapper;
        private readonly IMessage _messageService;

        public MemberClickService(IReadOnlyRepository readOnlyRepository,
                                  TrackingLinkGenerator trackingLinkGenerator,
                                  IdGeneratorFactory idGeneratorFactory,
                                  IWoolworthsEncryptionProvider woolworthsEncryptionProvider,
                                  IAmazonSimpleNotificationService snsClient,
                                  CommonConfig commonConfig,
                                  IMapper mapper, 
                                  IMessage messageService)
        {
            _readOnlyRepository = readOnlyRepository;
            _trackingLinkGenerator = trackingLinkGenerator;
            _idGeneratorFactory = idGeneratorFactory;
            _woolworthsEncryptionProvider = woolworthsEncryptionProvider;
            _snsClient = snsClient;
            _commonConfig = commonConfig;
            _mapper = mapper;
            _messageService = messageService;
        }

        private const int BupaPetInsurance_MerchantId = 1003801;
        private static readonly string SkyScannerMerchantName = "Skyscanner";

        public async Task<TrackingLinkResultModel> GetMemberClickWithTrackingUrlAsync(TrackingLinkInfoModel trackingInfo)
        {
            TrackingLinkResultModel clickTracking = null;
            SplitTrackingLinkInfoFromRawString(ref trackingInfo);

            if (trackingInfo != null)
            {
                trackingInfo.MemberClickType = GetMemberClickType(trackingInfo.MemberClickItemTypeString);
                switch (trackingInfo.MemberClickType)
                {
                    case MemberClickItemTypeEnum.Merchant:
                        clickTracking = await GetMemberClickByMerchantAsync(trackingInfo);
                        break;
                    case MemberClickItemTypeEnum.Offer:
                        clickTracking = await GetMemberClickByOfferAsync(trackingInfo);
                        break;
                    case MemberClickItemTypeEnum.MerchantAlias:
                        clickTracking = await GetMemberClickByMerchantAliasAsync(trackingInfo);
                        break;
                    case MemberClickItemTypeEnum.MerchantTier:
                        clickTracking = await GetMemberClickByMerchantTierAsync(trackingInfo);
                        break;
                    default:
                        break;
                }
            }

            clickTracking.TrackingLink = (trackingInfo.IsMobileApp && trackingInfo.ClientId == Constants.Clients.CashRewards && clickTracking.MerchantMobileAppTrackingType == MobileAppTrackingTypeEnum.AppToApp && trackingInfo.Merchant.MobileTrackingNetwork != (int)MobileTrackingNetworkEnum.Deeplink)
                ? clickTracking.MerchantWebsiteUrl
                : clickTracking.TrackingLink;

            return clickTracking;
        }

        public async Task<MemberClickTypeDetailsResultModel> GetMemberClickTypeDetails(string hyphenatedStringWithType, int clientId)
        {
            MemberClickTypeDetailsResultModel memberClickTypeDetailsResultModel = null;
            int tokenIndex = ValidateHyphenatedStringWithType(hyphenatedStringWithType);
            string hyphenatedString = hyphenatedStringWithType.Substring(0, tokenIndex);
            string memberClickItemTypeString = hyphenatedStringWithType.Substring(tokenIndex + 1);

            MemberClickItemTypeEnum memberClickType = GetMemberClickType(memberClickItemTypeString);
            switch (memberClickType)
            {
                case MemberClickItemTypeEnum.Merchant:
                    memberClickTypeDetailsResultModel= await GetMemberClickTypeDetailsByMerchantHyphenatedString(hyphenatedString, clientId);
                    break;
                case MemberClickItemTypeEnum.Offer:
                    memberClickTypeDetailsResultModel = await GetMemberClickTypeDetailsByOfferHyphenatedString(hyphenatedString, clientId);
                    break;
                case MemberClickItemTypeEnum.MerchantTier:
                    memberClickTypeDetailsResultModel = await GetMemberClickTypeDetailsByMerchantTierHyphenatedString(hyphenatedString, clientId);
                    break;
                default:
                    break;
            }

            return memberClickTypeDetailsResultModel;
        }

        private async Task<MemberClickTypeDetailsResultModel> GetMemberClickTypeDetailsByMerchantHyphenatedString(string hyphenatedString, int clientId)
        {
            MerchantModel merchant = await GetMerchantByHyphenatedNameAsync(clientId,
               hyphenatedString, false);
            return BuildMemberClickTypeDetailsResult(hyphenatedString, merchant.IsPaused);          
        }

        private async Task<MemberClickTypeDetailsResultModel> GetMemberClickTypeDetailsByOfferHyphenatedString(string hyphenatedString, int clientId)
        {
            OfferModel offer = await GetOfferByHyphenatedStringAsync(clientId, null, hyphenatedString);
            return BuildMemberClickTypeDetailsResult(offer.MerchantHyphenatedString, offer.IsMerchantPaused);
        }

        private async Task<MemberClickTypeDetailsResultModel> GetMemberClickTypeDetailsByMerchantTierHyphenatedString(string hyphenatedString, int clientId)
        {
            var index = hyphenatedString.LastIndexOf('-');
            var merchantName = hyphenatedString.Substring(0, index);

            MerchantModel merchant = await GetMerchantByHyphenatedNameAsync(clientId,
              merchantName, false);
            return BuildMemberClickTypeDetailsResult(merchantName, merchant.IsPaused);
        }

        private static MemberClickTypeDetailsResultModel BuildMemberClickTypeDetailsResult(string hyphenatedString, bool paused)
        {
            return new MemberClickTypeDetailsResultModel
            {
                IsMerchantPaused = paused,
                MerchantHyphenatedString = hyphenatedString
            };
        }

        private void SplitTrackingLinkInfoFromRawString(ref TrackingLinkInfoModel trackingLinkInfo)
        {
            int tokenIndex = ValidateHyphenatedStringWithType(trackingLinkInfo?.HyphenatedStringWithType);
            trackingLinkInfo.HyphenatedString = trackingLinkInfo.HyphenatedStringWithType.Substring(0, tokenIndex);
            trackingLinkInfo.MemberClickItemTypeString = trackingLinkInfo.HyphenatedStringWithType.Substring(tokenIndex + 1);
        }

        private static int ValidateHyphenatedStringWithType(string hyphenatedStringWithType)
        {
            if (string.IsNullOrWhiteSpace(hyphenatedStringWithType))
            {
                throw new ArgumentNullException(nameof(hyphenatedStringWithType));
            }

            int tokenIndex = hyphenatedStringWithType.LastIndexOf('-');
            if (tokenIndex == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(hyphenatedStringWithType), $"{nameof(hyphenatedStringWithType)} shuold contain -.");
            }
            if (hyphenatedStringWithType.Length <= tokenIndex + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hyphenatedStringWithType), $"click type identifier should exist after -.");
            }

            return tokenIndex;
        }

        private MemberClickItemTypeEnum GetMemberClickType(string itemTypeString)
        {
            switch (itemTypeString.ToLowerInvariant())
            {
                case "m":
                    {
                        return MemberClickItemTypeEnum.Merchant;
                    }
                case "ma":
                    {
                        return MemberClickItemTypeEnum.MerchantAlias;
                    }
                case "o":
                    {
                        return MemberClickItemTypeEnum.Offer;
                    }
                case "mt":
                    {
                        return MemberClickItemTypeEnum.MerchantTier;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(itemTypeString), itemTypeString, $"{nameof(itemTypeString)} should be m, ma or o");
                    }

            }
        }

        private async Task<TrackingLinkResultModel> GetMemberClickByMerchantAsync(TrackingLinkInfoModel trackingInfo)
        {
            MerchantModel merchant = await GetMerchantByHyphenatedNameAndClientIdsAsync(trackingInfo.ClientId, trackingInfo.PremiumClientId,
                trackingInfo.HyphenatedString, trackingInfo.IsMobileApp);

            if (merchant != null)
            {
                trackingInfo.Merchant = merchant;
                trackingInfo.ClickItemId = merchant.MerchantId;
                trackingInfo.ClickItemImageUrl = merchant.RegularImageUrl;
                trackingInfo.TrackingLinkTemplate = merchant.TrackingLink;
                if (trackingInfo.PremiumClientId.HasValue)
                    trackingInfo.PremiumMerchant = await GetMerchantByHyphenatedNameAsync(trackingInfo.PremiumClientId.Value, trackingInfo.HyphenatedString, trackingInfo.IsMobileApp);

                return await GetRedirectLinkFromTrackingAsync(trackingInfo);
            }

            throw new NotFoundException("Merchant information not found on server.");
        }

        private async Task<TrackingLinkResultModel> GetMemberClickByOfferAsync(TrackingLinkInfoModel trackingInfo)
        {
            var offer = await GetOfferByHyphenatedStringAsync(trackingInfo.ClientId, trackingInfo.PremiumClientId, trackingInfo.HyphenatedString);
            trackingInfo.ClickItemId = offer.OfferId;
            trackingInfo.ClickItemImageUrl = offer.RegularImageUrl;


            if (offer != null && !string.IsNullOrEmpty(offer.MerchantHyphenatedString))
            {
                trackingInfo.Merchant = await GetMerchantByHyphenatedNameAsync(trackingInfo.ClientId, offer.MerchantHyphenatedString, trackingInfo.IsMobileApp);
                if (trackingInfo.PremiumClientId.HasValue)
                    trackingInfo.PremiumMerchant = await GetMerchantByHyphenatedNameAsync(trackingInfo.PremiumClientId.Value, offer.MerchantHyphenatedString, trackingInfo.IsMobileApp);

                if (trackingInfo.Merchant == null)
                {
                    throw new Exception("Merchant information not found on server.");
                }
                trackingInfo.TrackingLinkTemplate = string.IsNullOrEmpty(offer.TrackingLink) ? offer.MerchantTrackingLink : offer.TrackingLink;
                return await GetRedirectLinkFromTrackingAsync(trackingInfo);
            }

            throw new NotFoundException("Offer information not found on server.");
        }

        private async Task<TrackingLinkResultModel> GetMemberClickByMerchantAliasAsync(TrackingLinkInfoModel trackingInfo)
        {
            var merchantAlias = await GetMerchantAliaseAsync(trackingInfo.ClientId, trackingInfo.HyphenatedString);
            trackingInfo.ClickItemId = merchantAlias.MerchantAliasId;
            trackingInfo.ClickItemImageUrl = merchantAlias.RegularImageUrl;

            if (merchantAlias != null)
            {
                trackingInfo.Merchant = await GetMerchantById(merchantAlias.MerchantId);
                if (trackingInfo.PremiumClientId.HasValue)
                    trackingInfo.PremiumMerchant = await GetMerchantByHyphenatedNameAsync(trackingInfo.PremiumClientId.Value, trackingInfo.HyphenatedString, trackingInfo.IsMobileApp);

                return await GetRedirectLinkFromTrackingAsync(trackingInfo);
            }

            throw new NotFoundException("Merchant alias information not found on server.");
        }

        private async Task<TrackingLinkResultModel> GetMemberClickByMerchantTierAsync(TrackingLinkInfoModel trackingInfo)
        {
            string hyphen = trackingInfo.HyphenatedString;
            var index = hyphen.LastIndexOf('-');
            var merchantName = hyphen.Substring(0, index);
            if (!int.TryParse(hyphen.Substring(index + 1), out int tierId))
            {
                throw new Cashrewards3API.Exceptions.ArgumentException("MerchantTierId must be int");
            }

            MerchantModel merchant = await GetMerchantByHyphenatedNameAndClientIdsAsync(trackingInfo.ClientId, trackingInfo.PremiumClientId, 
                merchantName, trackingInfo.IsMobileApp);

            var tier = await GetMerchantTierById(tierId);
            if (merchant == null || tier == null)
                throw new Cashrewards3API.Exceptions.NotFoundException("Merchant Tier information not found on server.");

            trackingInfo.Merchant = merchant;
            trackingInfo.ClickItemImageUrl = merchant.RegularImageUrl;
            trackingInfo.ClickItemId = tier.MerchantTierId;
            trackingInfo.TrackingLinkTemplate = tier.TrackingLink;
            if (trackingInfo.PremiumClientId.HasValue)
                trackingInfo.PremiumMerchant = await GetMerchantByHyphenatedNameAsync(trackingInfo.PremiumClientId.Value, merchantName, trackingInfo.IsMobileApp);

            return await GetRedirectLinkFromTrackingAsync(trackingInfo);
        }

        private async Task<TrackingLinkResultModel> GetRedirectLinkFromTrackingAsync(TrackingLinkInfoModel trackingInfo)
        {
            trackingInfo.Network = await GetNetworkByIdAsync(trackingInfo.Merchant.NetworkId);

            var trackingId = GenerateTrackingId(trackingInfo.Merchant.NetworkId, trackingInfo.Member.MemberId, trackingInfo.ClientId);

            var trackingRef = this.GetTrackingRef(trackingId, trackingInfo.Merchant.MerchantId);

            string redirectionLinkedUsed;

            if (trackingInfo.Merchant.MerchantName == SkyScannerMerchantName)
            {
                // BAU-760 special handling for skyscanner
                redirectionLinkedUsed = this.GetSkyscannerRedirectionLink(trackingInfo, trackingRef);
            }
            else
            {
                redirectionLinkedUsed = GenerateTrackingUrl(trackingInfo, trackingRef);
            }

            TrackingLinkResultPremiumMerchant premium = null;
            if (trackingInfo.PremiumMerchant != null)
            {
                premium = new TrackingLinkResultPremiumMerchant()
                {
                    ClientCommissionString = trackingInfo?.PremiumMerchant?.ClientCommissionString
                };
            };

            var merchantTiers = await GetMerchantTiersByClientIdAndMerchantId(
                        trackingInfo.ClientId, trackingInfo.PremiumClientId, trackingInfo.Merchant.MerchantId);
            var tiers = MapPremiumTiersToTiers(merchantTiers.Item1, merchantTiers.Item2);
            var trackingLink = new TrackingLinkResultModel()
            {
                FirstName = trackingInfo.Member.FirstName,
                MerchantName = trackingInfo.Merchant.MerchantName,
                ClientCommissionString = trackingInfo.PremiumMerchant?.ClientCommissionString ?? trackingInfo.Merchant.ClientCommissionString,
                MerchantId = trackingInfo.Merchant.MerchantId,
                MerchantImageUrl = trackingInfo.ClickItemImageUrl,
                MerchantWebsiteUrl = trackingInfo.Merchant.WebsiteUrl,
                MerchantMobileAppTrackingType = trackingInfo.Merchant.MobileAppTrackingType,
                NetworkId = trackingInfo.Merchant.NetworkId,
                ClickItemId = trackingInfo.ClickItemId,
                TrackingLink = redirectionLinkedUsed,
                TrackingId = trackingId,
                Premium = premium,
                Tiers = trackingInfo.IncludeTiers ? tiers.ToList() : null
            };

            await SetWwEncryption(trackingLink, trackingInfo);

            await SendMemberClickCreateEvent(
                 CreateMemberClickCreateUpdateEvent(trackingInfo, redirectionLinkedUsed, trackingId)
                 );

            var hasMemberClick = await IsFirstMemberClickAsync(trackingInfo.Member.MemberId);
            if (!hasMemberClick)
            {
                await SendFirstMemberClickEmailAsync(trackingInfo.Member.MemberId, trackingInfo.Merchant.MerchantName, trackingInfo.Merchant.RegularImageUrl);
            }

            return trackingLink;
        }

        private async Task SendFirstMemberClickEmailAsync(int memberId, string merchantName, string merchantImageUrl)
        {

            var message = new MemberFirstClickEvent
            {
                TargetId = memberId.ToString(),
                EmailType = (int)EmailTypeEnum.MemberFirstClick,
                AdditionalInfo = merchantName,
                MerchantImageUrl = merchantImageUrl
            };

            await _messageService.MemberFirstClickEvent(message);
        }

        private async Task<bool> IsFirstMemberClickAsync(int memberId)
        {
            var query = @"SELECT count(ClickId) AS TotalCount
                                FROM MEMBERCLICKS MC
                                WHERE MEMBERID = @MemberId;";

            var totalCount = await _readOnlyRepository.QueryFirstOrDefault<int>(query, new
            {
                memberId
            });
            return totalCount > 0;
        }

        private string GenerateTrackingId(int networkId, int memberId, int clientId)
        {
            return _idGeneratorFactory.GetService(networkId)?.GetUniqueId(memberId, clientId);
        }

        private string GetTrackingRef(string trackingId, int merchantId)
        {
            return merchantId == BupaPetInsurance_MerchantId ? trackingId.Remove(0, 1) : trackingId;
        }

        private string GenerateTrackingUrl(TrackingLinkInfoModel trackingInfo, string trackingRef)
        {
            switch (trackingInfo.MemberClickType)
            {
                case MemberClickItemTypeEnum.Merchant:
                case MemberClickItemTypeEnum.Offer:
                case MemberClickItemTypeEnum.MerchantTier:
                    {
                        return _trackingLinkGenerator.GenerateTrackingLinkByNetwork(trackingInfo.TrackingLinkTemplate, trackingInfo.Network, trackingRef, trackingInfo.Member.MemberId, trackingInfo.ClientId);
                    }

                case MemberClickItemTypeEnum.MerchantAlias:
                    {
                        return _trackingLinkGenerator.GenerateTrackingLinkForAliasByNetwork(trackingInfo.TrackingLinkTemplate, trackingInfo.Network, trackingRef, trackingInfo.Network.DeepLinkHolder);
                    }

                default:
                    {
                        return string.Empty;
                    }
            }
        }

        private string GetSkyscannerRedirectionLink(TrackingLinkInfoModel trackingInfo, string trackingRef)
        {
            //https://www.awin1.com/cread.php?awinmid=16286&awinaffid=211491&clickref=MemberId&clickref2=ClickID
            if (!string.IsNullOrEmpty(trackingInfo.TrackingLinkTemplate))
            {
                return trackingInfo.TrackingLinkTemplate.Replace("clickref=", $"clickref={trackingInfo.Member.MemberId}")
                    .Replace("clickref2=", $"clickref2={trackingRef}");
            }

            return string.Empty;
        }

        private MemberClickCreateUpdateEvent CreateMemberClickCreateUpdateEvent(TrackingLinkInfoModel data, string redirectionUrl, string trackingId, bool isUpdate = false)
        {
            return new MemberClickCreateUpdateEvent
            {
                CampaignId = data.CampaignId,
                CashbackOffer = data.PremiumMerchant?.ClientCommissionString ?? data.Merchant.ClientCommissionString,
                ItemId = data.ClickItemId,
                ItemType = data.MemberClickType.GetDescription(),
                MemberId = data.Member.MemberId,
                MerchantId = data.Merchant.MerchantId,
                MerchantName = data.Merchant.MerchantName,
                NetworkId = data.Network.NetworkId,
                NetworkName = data.Network.NetworkName,
                TenantId = data.ClientId,
                IPAddress = data.IpAddress,
                DateCreated = DateTime.Now.ToUniversalTime(),
                UserAgent = data.UserAgent,
                TrackingId = trackingId,
                RedirectionLinkUsed = redirectionUrl,
                Update = isUpdate
            };
        }

        private async Task SetWwEncryption(TrackingLinkResultModel trackingLink, TrackingLinkInfoModel trackingInfo)
        {
            if (trackingInfo.Merchant.MerchantId == Constants.Merchants.Woolworths || trackingInfo.Merchant.MerchantId == Constants.Merchants.Masters || trackingInfo.Merchant.MerchantId == Constants.Merchants.BigW)
            {
                WoolworthsEncryptionModel encryptionDetails = await _woolworthsEncryptionProvider.GetWoolworthsEncryptionDetails(trackingInfo.ClientId, trackingInfo.Merchant.MerchantId, trackingInfo.Member.MemberId);
                trackingLink.WwEncryptedClientMemberId = encryptionDetails.WwEncryptedClientMemberId;
                trackingLink.WwEncryptedSiteReferenceId = encryptionDetails.WwEncryptedSiteReferenceId;
                trackingLink.WwEncryptedTimeStamp = encryptionDetails.WwEncryptedTimeStamp;
            }
        }

        private async Task SendMemberClickCreateEvent(MemberClickCreateUpdateEvent message)
        {
            var request = new PublishRequest
            {
                Message = JsonConvert.SerializeObject(message),
                TopicArn = _commonConfig.ClickCreateTopicArn,
                MessageStructure = "Raw",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType",
                        new MessageAttributeValue {DataType = "String", StringValue = message.GetType().FullName}
                    }
                }
            };

            await _snsClient.PublishAsync(request);
        }

        #region db query
        private async Task<MerchantModel> GetMerchantByHyphenatedNameAndClientIdsAsync(int clientId, int? premiumClientId, string hyphenatedString, bool isMobleApp)
        {
            var clientIds = new List<int>() { clientId };
            if (premiumClientId.HasValue)
                clientIds.Add(premiumClientId.Value);

            var queryString = @"SELECT [MetaDescription] ,[DescriptionShort] ,[DescriptionLong] ,[BasicTerms] ,[ExtentedTerms] ,[MerchantId] ,[IsLatest] ,[NetworkId] ,[MerchantName] ,[IsFeatured] ,[IsPopular] ,[IsHomePageFeatured] ,[HyphenatedString] ,[RegularImageUrl] ,[SmallImageUrl] ,[MediumImageUrl] ,[RegularImageUrlSecure] ,[SmallImageUrlSecure] ,[MediumImageUrlSecure] ,[RandomNumber] ,[ClientId] ,[TierCommTypeId] ,[Commission] ,[ClientComm] ,[MemberComm] ,[TierTypeId] ,[TierCssClass] ,[TrackingLink] ,[IsExtra] ,[FlagImageUrl] ,[OfferCount] ,[RewardName] ,[ClientProgramTypeId] ,[TierDescription] ,[TierName] ,[WebsiteUrl] ,[TrackingTime] ,[ApprovalTime] ,[Rate] ,[NotificationMsg] ,[ConfirmationMsg] ,[MobileEnabled] ,[TierCount] ,[IsToolbarEnabled] ,[IsLuxuryBrand] ,[ReferenceName] ,[MobileAppTrackingType] ,[IsFlatRate] ,[IsMobileAppEnabled] ,[WebsiteUrlPattern] ,[MerchantBadgeCode] ,[BackgroundImageUrl]  ,[IsToolbarOptoutSearch] ,[IsToolbarOptoutSerps] ,[IsToolbarOptoutSliderActivation] ,[DesktopEnabled] ,[MobileTrackingNetwork] ,[WebsiteUrlPatternSlider] ,[IsPremiumDisabled]
                                FROM dbo.MaterialisedMerchantView 
                                WHERE ClientId IN @ClientIds AND HyphenatedString = @HyphenatedString AND NetworkId NOT IN @InStoreNetworks";

            var merchantList = await _readOnlyRepository.Query<MerchantModel>(queryString, new
            {
                ClientIds = clientIds,
                HyphenatedString = hyphenatedString,
                InStoreNetworks = Constants.Networks.InStoreNetworkIds
            });

            List<MerchantModel> merchants = null;
            if (premiumClientId.HasValue)
            {
                merchants = merchantList.Where(m => m.ClientId == premiumClientId.Value).ToList();
            }
            if (!(merchants?.Any() ?? false))
            {
                merchants = merchantList.Where(m => m.ClientId == clientId).ToList();
            }

            MerchantModel merchant = null;
            if (isMobleApp)
            {
                merchant = merchants.FirstOrDefault(x => Constants.Networks.MobileSpecificNetworkIds.Contains(x.NetworkId));
            }

            return merchant ?? merchants.FirstOrDefault(x => !Constants.Networks.MobileSpecificNetworkIds.Contains(x.NetworkId));
        }

        private async Task<MerchantModel> GetMerchantByHyphenatedNameAsync(int clientId, string hyphenatedString, bool isMobleApp)
        {
            var queryString = @"SELECT * 
                                FROM dbo.MaterialisedMerchantView 
                                WHERE ClientId = @ClientId AND HyphenatedString = @HyphenatedString AND NetworkId NOT IN @InStoreNetworkIds";

            IEnumerable<MerchantModel> merchantList = await _readOnlyRepository.Query<MerchantModel>(queryString, new
            {
                ClientId = clientId,
                HyphenatedString = hyphenatedString,
                InStoreNetworkIds = Constants.Networks.InStoreNetworkIds
            });

            MerchantModel merchant = null;
            if (isMobleApp)
            {
                merchant = merchantList.FirstOrDefault(x => Constants.Networks.MobileSpecificNetworkIds.Contains(x.NetworkId));
            }

            return merchant ?? merchantList.FirstOrDefault(x => !Constants.Networks.MobileSpecificNetworkIds.Contains(x.NetworkId));
        }

        private async Task<OfferModel> GetOfferByHyphenatedStringAsync(int clientId, int? premiumClientId , string hyphenatedString)
        {
            var clientIds = new List<int>() { clientId };
            if (premiumClientId.HasValue)
                clientIds.Add(premiumClientId.Value);

            var queryString = @"SELECT * 
                                FROM dbo.MaterialisedOfferView 
                                WHERE ClientId IN @ClientIds AND HyphenatedString = @HyphenatedString";

            var offerModels = await _readOnlyRepository.Query<OfferModel>(queryString, new
            {
                ClientIds = clientIds,
                HyphenatedString = hyphenatedString
            });

            List<OfferModel> offers = null;
            if (premiumClientId.HasValue)
            {
                offers = offerModels.Where(m => m.ClientId == premiumClientId.Value).ToList();
            }
            if (!(offers?.Any() ?? false))
            {
                offers = offerModels.Where(m => m.ClientId == clientId).ToList();
            }

            return offers.FirstOrDefault();
        }

        private async Task<MerchantAliasModel> GetMerchantAliaseAsync(int clientId, string hyphenatedString)
        {
            var queryString = @"SELECT * 
                                FROM dbo.MerchantAliasView 
                                WHERE Status = 1 AND HyphenatedString = @HyphenatedString AND IsFeatured=1
                                ORDER BY TopFeatured, MerchantName";

            MerchantAliasModel merchantAlias;
            merchantAlias = await _readOnlyRepository.QueryFirstOrDefault<MerchantAliasModel>(queryString, new
            {
                HyphenatedString = hyphenatedString,
            });

            return merchantAlias;

        }

        private async Task<NetworkModel> GetNetworkByIdAsync(int networkId)
        {
            var queryString = @"SELECT * 
                                FROM dbo.Network 
                                WHERE NetworkId= @NetworkId";

            NetworkModel network;
            network = await _readOnlyRepository.QueryFirstOrDefault<NetworkModel>(queryString, new
            {
                NetworkId = networkId,
            });

            return network;
        }

        private async Task<MerchantModel> GetMerchantById(int merchantId)
        {
            var queryString = @"SELECT * 
                                FROM dbo.MaterialisedMerchantView 
                                WHERE MerchantId = @MerchantId";

            MerchantModel merchant;
            merchant = await _readOnlyRepository.QueryFirstOrDefault<MerchantModel>(queryString, new
            {
                MerchantId = merchantId,
            });

            return merchant;
        }

        private async Task<Tuple<List<MerchantTier>, List<MerchantTier>>> GetMerchantTiersByClientIdAndMerchantId(int clientId, int? premiumClientId, int merchantId)
        {
            var clientIds = new List<int>() { clientId };
            if (premiumClientId.HasValue)
                clientIds.Add(premiumClientId.Value);

            var queryString = @"SELECT * 
                                FROM dbo.MaterialisedMerchantTierView
                                WHERE ClientId in @ClientId AND MerchantId = @MerchantId";

            var merchantTiersView = await _readOnlyRepository.Query<MerchantTierView>(queryString, new
            {
                ClientId = clientIds,
                MerchantId = merchantId,
            });
            merchantTiersView = merchantTiersView.Where(mt => mt.TierTypeId != (int)TierTypeEnum.Hidden).ToList();
            var standardMerchantTiers = _mapper.Map<List<MerchantTier>>(merchantTiersView.Where(t => t.ClientId == clientId).ToList());
            List<MerchantTier> premiumMerchantTiers = null;
            if (premiumClientId.HasValue)
                premiumMerchantTiers = _mapper.Map<List<MerchantTier>>(merchantTiersView.Where(t => t.ClientId == premiumClientId.Value).ToList());

            return Tuple.Create(standardMerchantTiers, premiumMerchantTiers);
        }

        private IEnumerable<TrackingLinkResultMerchantTier> MapPremiumTiersToTiers(IEnumerable<MerchantTier> standardTiers, IEnumerable<MerchantTier> premiumTiers)
        {
            var tiers = standardTiers.ToDictionary(
                tier => tier.MerchantTierId, tier => _mapper.Map<TrackingLinkResultMerchantTier>(tier));

            if (premiumTiers != null)
                foreach (var premiumTier in premiumTiers)
                {
                    if (tiers.TryGetValue(premiumTier.MerchantTierId, out var merchantTier))
                    {
                        merchantTier.Premium = _mapper.Map<TrackingLinkResultMerchantTierPremium>(premiumTier);
                    }
                    else
                    {
                        var preimumTrackingLinkResultMerchantTierMerchantTier = _mapper.Map<TrackingLinkResultMerchantTier>(premiumTier);
                        preimumTrackingLinkResultMerchantTierMerchantTier.Premium =
                            _mapper.Map<TrackingLinkResultMerchantTierPremium>(preimumTrackingLinkResultMerchantTierMerchantTier);
                        tiers.Add(premiumTier.MerchantTierId, preimumTrackingLinkResultMerchantTierMerchantTier);
                    }
                }

            return tiers.Values;
        }

        private async Task<MerchantTierView> GetMerchantTierById(int tierId)
        {
         var queryString = @"SELECT MerchantTierId, TrackingLink 
                       FROM dbo.MerchantTier 
                       WHERE MerchantTierId = @MerchantTierId";

         return await _readOnlyRepository.QueryFirstOrDefault<MerchantTierView>(queryString, new
            {
                MerchantTierId = tierId,
            });

        }

        #endregion
    }
}
