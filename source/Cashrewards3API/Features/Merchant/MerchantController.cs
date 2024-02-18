using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Member.Models;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Cashrewards3API.Features.Merchant
{
    [Authorize]
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class MerchantController : ControllerBase
    {
        #region Constructor(s)

        private readonly IMerchantService _merchantService;
        private readonly IMerchantBundleService _merchantBundleService;
        private readonly IPopularMerchantService _popularMerchantsvc;
        private readonly ITrendingMerchantService _trendingService;
        private readonly IPremiumService _premiumService;
        private readonly ILogger<MerchantController> _logger;
        private readonly IRequestContext _requestContext;
        private readonly IFeatureToggle _featureToggle;

        public MerchantController(
            IRequestContext requestContext,
            IMerchantService merchantService,
            IMerchantBundleService merchantBundleService,
            IPopularMerchantService popularMerchantsvc,
            ITrendingMerchantService trendingService,
            IPremiumService premiumService,
            ILogger<MerchantController> logger,
            IFeatureToggle featureToggle
         )
        {
            _requestContext = requestContext;
            _merchantService = merchantService;
            _merchantBundleService = merchantBundleService;
            _popularMerchantsvc = popularMerchantsvc;
            _trendingService = trendingService;
            _premiumService = premiumService;
            _logger = logger;
            _featureToggle = featureToggle;
        }

        #endregion Constructor(s)

        /// <summary>
        /// Get paged list of merchants
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet()]
        [Route("merchants")]
        [ProducesResponseType(typeof(PagedList<MerchantBundleBasicModel>), 200)]
        public async Task<ActionResult<PagedList<MerchantBundleBasicModel>>> GetMerchantBundle(
                    string filter, int offset = 0, int limit = 20)
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;
            var result = ParseFilterString(filter);
            var merchantRequestInfoModel = new MerchantRequestInfoModel
            {
                ClientId = clientId,
                PremiumClientId = premiumClientId,
                CategoryId = result.CategoryId,
                InStoreFlag = result.InStoreFlag,
                Offset = offset,
                Limit = limit
            };
            var response = await _merchantService.GetMerchantBundleByFilterAsync(merchantRequestInfoModel);
            return Ok(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [AllowAnonymous]
        [Route("merchants/{merchantId:int}")]
        [ProducesResponseType(typeof(IEnumerable<MerchantBundleDetailResultModel>), 200)]
        public async Task<ActionResult<MerchantBundleDetailResultModel>> GetMerchantBundle(int merchantId)
        {
            if (merchantId <= 0)
            {
                throw new BadRequestException("Merchant Id is not valid");
            }

            (int clientId, int? premiumClientId) = _requestContext.ClientIds;

            var featureToggle = _featureToggle.DisplayFeature(Enum.FeatureNameEnum.Premium, premiumClientId);

            var response = await _merchantBundleService.GetMerchantBundleByIdAsync(clientId, merchantId, premiumClientId, _requestContext.IsMobileDevice);

            return Ok(response);
        }

        /// <summary>
        /// Get merchant bundle by hyphenatedString
        /// </summary>
        /// <param name="hyphenatedString"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [AllowAnonymous]
        [Route("merchants/storesbundle/{hyphenatedString}")]
        [ProducesResponseType(typeof(IEnumerable<MerchantStoresBundle>), 200)]
        public async Task<ActionResult<MerchantStoresBundle>> GetMerchantBundleByHyphenatedString(string hyphenatedString)
        {
            if (string.IsNullOrEmpty(hyphenatedString))
            {
                return BadRequest();
            }

            (int clientId, int? premiumClientId) = _requestContext.ClientIds;

            var message = await _merchantBundleService.GetMerchantStoresBundleByHyphenatedString(clientId, premiumClientId, hyphenatedString, _requestContext.IsMobileDevice);

            return Ok(message);
        }

        /// <summary>
        /// Get popular merchants for browser
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns>Get matter settings.</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet()]
        [AllowAnonymous]
        [Route("merchants/popular")]
        [ProducesResponseType(typeof(IEnumerable<MerchantDto>), 200)]
        public async Task<ActionResult<MerchantDto>> GetPopularMerchants(
            int offset = 0, int limit = 35)
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;
            var merchants = _requestContext.IsMobileDevice
                ? await _popularMerchantsvc.GetPopularMerchantsForMobileAsync(clientId, premiumClientId, offset, limit)
                : await _popularMerchantsvc.GetPopularMerchantsForBrowserAsync(clientId, premiumClientId, offset, limit);

            return Ok(merchants);
        }

        /// <summary>
        /// Get popular merchants for mobile
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet()]
        [Route("merchants/popular/mobile")]
        [ProducesResponseType(typeof(IEnumerable<MerchantDto>), 200)]
        public async Task<ActionResult<MerchantDto>> GetPopularMerchantsForMobile(
            int offset = 0, int limit = 48)
        {
            int clientId = await _requestContext.GetClientIdFromDynamoDbAsync();

            return Ok(await _popularMerchantsvc.GetPopularMerchantsForMobileAsync(clientId, null, offset, limit));
        }

        /// <summary>
        /// Get trending merchants for browser
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet()]
        [AllowAnonymous]
        [Route("merchants/trending")]
        [ProducesResponseType(typeof(PagedList<MerchantDto>), 200)]
        public async Task<ActionResult<PagedList<MerchantDto>>> GetTrendingStoresForBrowser(
            string filter = null, int offset = 0, int limit = 12)
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;
            var categoryId = 0;
            if (!string.IsNullOrEmpty(filter))
            {
                categoryId = ParseFilterString(filter).CategoryId;
            }

            var merchants = _requestContext.IsMobileDevice
                ? await _trendingService.GetTrendingStoresForMobile(clientId, premiumClientId, categoryId, offset, limit)
                : await _trendingService.GetTrendingStoresForBrowser(clientId, premiumClientId, categoryId, offset, limit);

            if(this._featureToggle.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED))
            {
                FilterPausedMerchants(merchants);
            }
            return Ok(merchants);
        }

        private void FilterPausedMerchants(PagedList<MerchantDto> pagedMerchants)
        {
            if (pagedMerchants == null || pagedMerchants.Data == null)
            {
                return;
            }

            pagedMerchants.Data=pagedMerchants.Data.Where(p => !p.IsPaused).ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet()]
        [Route("merchants/trending/mobile")]
        [ProducesResponseType(typeof(PagedList<MerchantDto>), 200)]
        public async Task<ActionResult<PagedList<MerchantDto>>> GetTrendingStoresForMobile(
           string filter = null, int offset = 0, int limit = 12)
        {
            int clientId = await _requestContext.GetClientIdFromDynamoDbAsync();
            var categoryId = 0;
            if (!string.IsNullOrEmpty(filter))
            {
                categoryId = ParseFilterString(filter).CategoryId;
            }

            var merchants = await _trendingService.GetTrendingStoresForMobile(
                clientId, null, categoryId, offset, limit);

            if (this._featureToggle.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED))
            {
                FilterPausedMerchants(merchants);
            }

            return Ok(merchants);
        }

        /// <summary>
        /// Get paged list of all merchants for the all stores page
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet()]
        [Route("merchants/all-stores")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedList<AllStoresMerchantModel>), 200)]
        public async Task<ActionResult<PagedList<AllStoresMerchantModel>>> GetMerchantBundle(
                    int categoryId, int offset = 0, int limit = 20)
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;
            var response = await _merchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, categoryId, offset, limit, premiumClientId);

            return Ok(response);
        }

        // Always returns camel case for backward compatibility
        [HttpGet()]
        [Route("public/merchants/all-stores")]
        [AllowAnonymous]
        [CamelCase]
        [ProducesResponseType(typeof(PagedList<AllStoresMerchantModel>), 200)]
        public async Task<ActionResult<PagedList<AllStoresMerchantModel>>> GetMerchantBundlePublic(
                    int categoryId, int offset = 0, int limit = 20)
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;
            var response = await _merchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, categoryId, offset, limit, premiumClientId);

            return Ok(response);
        }

        private dynamic ParseFilterString(string filterString)
        {
            return FilterParser.Parse(filterString);
        }
    }
}