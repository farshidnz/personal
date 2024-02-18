using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Proxies.Models;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cashrewards3API.Features.Proxies
{  
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public class ProxiesController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly IAddCardService _addCardService;
        private readonly IProxyApiService _proxyApiService;
        private readonly IHttpClientFactory _clientFactory;

        public ProxiesController(ISearchService searchService, 
            IAddCardService addCardService,
            IProxyApiService proxyApiService,
            IHttpClientFactory clientFactory)
        {
            _searchService = searchService;
            _addCardService = addCardService;
            _proxyApiService = proxyApiService;
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Get search results
        /// </summary>
        /// <param name="q">Query request</param>
        /// <param name="sort">Sorting by relevance/cashback/merchantname. ( format = [fieldname]:[asc/desc] | example \"cashback:desc,merchantname:asc\</param>
        /// <param name="per_page">Specify the number of records to return in one request</param>
        /// <param name="page">specify the page of results to return (start from 1)</param>
        /// <param name="_clientid">ClientId</param>
        /// <param name="_token">JWT Access token</param>
        /// <param name="fields">Required fields, comma separated</param>
        /// <returns>Search response</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<SummaryModel>), 200)]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string sort,
            [FromQuery] int per_page, [FromQuery] int page, [FromQuery] string _clientid, [FromQuery] string _token, [FromQuery] string fields)
        {
            var queryString = Request.QueryString;
            var proxyResponse = await _searchService.GetSearchResult(queryString.ToString());
            if (proxyResponse.StatusCode == HttpStatusCode.OK)
            {
                var reponse = JsonConvert.DeserializeObject<SummaryModel>(proxyResponse.ResponseText);
                return Ok(reponse);
            }

            var resp = JsonConvert.DeserializeObject<ErrorModel>(proxyResponse.ResponseText);
            return BadRequest(resp);
        }

        [HttpPost("addcard")]
        public async Task AddCard(AddCardRequestModel requestModel)
        {
            await _addCardService.AddCardAsync(requestModel);
        }

        /// <summary>
        ///  Get merchant mapping details from mastercard auth merchant id.
        /// </summary>
        /// <param name="authMerchantId">mastercard auth merchant id</param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("merchant/auth-id/{authMerchantId}")]
        [ProducesResponseType(typeof(IEnumerable<MerchantMappingResultModel>), 200)]
        public async Task MerchantMapByAuthId(string authMerchantId)
        {
            var client = _clientFactory.CreateClient("merchantmap-authid");
            var targetUri = new Uri(client.BaseAddress + $"{authMerchantId}");

            var targetRequestMessage = _proxyApiService.CreateRequestMessage(HttpContext, targetUri);
            using (var responseMessage = await client.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted))
            {
                HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                _proxyApiService.CopyFromTargetResponseHeaders(HttpContext, responseMessage);
                await responseMessage.Content.CopyToAsync(HttpContext.Response.Body);
            }
        }


        /// <summary>
        ///  Get merchant mapping details from mastercard merchant location id.
        /// </summary>
        /// <param name="locationId">mastercard merchant location id</param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet("merchant/location-id/{locationId:int}")]
        [ProducesResponseType(typeof(IEnumerable<MerchantMappingResultModel>), 200)]
        public async Task MerchantMapByLocationId(int locationId)
        {
            var client = _clientFactory.CreateClient("merchantmap-locationid");
            var targetUri = new Uri(client.BaseAddress + $"{locationId}");

            var targetRequestMessage = _proxyApiService.CreateRequestMessage(HttpContext, targetUri);
            using (var responseMessage = await client.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted))
            {
                HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                _proxyApiService.CopyFromTargetResponseHeaders(HttpContext, responseMessage);
                await responseMessage.Content.CopyToAsync(HttpContext.Response.Body);
            }

            return;
        }

    }

    

    
}
