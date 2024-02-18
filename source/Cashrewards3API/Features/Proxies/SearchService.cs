using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Cashrewards3API.Features.Proxies.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;


namespace Cashrewards3API.Features.Proxies
{
    public interface ISearchService
    {
        Task<ProxyResponse> GetSearchResult(string queryString);

    }
    public class SearchService : ISearchService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        
        
        public SearchService(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory clientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientFactory = clientFactory;
        }

        public async Task<ProxyResponse> GetSearchResult(string queryString)
        {
            var httpClient = _clientFactory.CreateClient("search");
            var mobileAppFilter = "&mobileapp=1";
            var searchRequest = new HttpRequestMessage(HttpMethod.Get, queryString.ToString() + mobileAppFilter);
            searchRequest.Headers.Clear();
            
            string accessToken = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(accessToken))
            {
                if (accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    accessToken = accessToken.Substring("Bearer ".Length).Trim();
                }
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            using (var response = await httpClient.SendAsync(searchRequest))
            {
                return new ProxyResponse()
                {
                    StatusCode = response.StatusCode,
                    ResponseText = await response.Content.ReadAsStringAsync()
                };
            }
        }


    }
}
