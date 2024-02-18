using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Proxies.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Cashrewards3API.Features.Proxies
{
    public interface IAddCardService
    {
        //Task<ProxyResponse> AddCardAsync(AddCardRequestModel requestModel);
        Task AddCardAsync(AddCardRequestModel requestModel);
    }
    public class AddCardService : IAddCardService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IProxyApiService _proxyApiService;

        public AddCardService(IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory clientFactory, IProxyApiService proxyApiService)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientFactory = clientFactory;
            _proxyApiService = proxyApiService;
        }

        public async Task AddCardAsync(AddCardRequestModel requestModel)
        {
            var client = _clientFactory.CreateClient("addcard");
            var targetUri = new Uri(client.BaseAddress + "card/add");
            var targetRequestMessage = _proxyApiService.CreateRequestMessage(_httpContextAccessor.HttpContext, targetUri);
            targetRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestModel), Encoding.UTF8, "application/json");
            using (var responseMessage = await client.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, 
                _httpContextAccessor.HttpContext.RequestAborted))
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                _proxyApiService.CopyFromTargetResponseHeaders(_httpContextAccessor.HttpContext, responseMessage);
                await responseMessage.Content.CopyToAsync(_httpContextAccessor.HttpContext.Response.Body);
            }
        }
        
    }
}
