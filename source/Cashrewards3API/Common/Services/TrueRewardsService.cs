using AutoMapper;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Services.Model;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Member.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public class TrueRewardsService : ITokenService
    {
        public const string AuthTokenEndpoint = "API/fetch-tr-widget-auth";
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMapper _mapper;        
        private readonly CommonConfig _config;
        private readonly HttpClient _httpClient;
        public TrueRewardsService(
             IMapper mapper,
             CommonConfig config,
             IHttpClientFactory httpClientFactory)
        {
            _clientFactory = httpClientFactory;
            _httpClient = _clientFactory.CreateClient("truerewards"); ;
            _config = config;
            _mapper = mapper;                      
        }



        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="NotAuthorizedException">Email {context.Email} - Talkable not Authorized</exception>
        public async Task<TokenContext> GetToken(AuthnRequestContext context)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "apiKey", _config.TrueRewards.ApiKey },
                { "app", _config.TrueRewards.App },
                { "name", context.FullName },
                { "email", context.Email }
            };

            HttpResponseMessage response = await _httpClient.PostAsync(AuthTokenEndpoint, new FormUrlEncodedContent(parameters));

            if (response.IsSuccessStatusCode)
            {
                var resp = await response.Content.ReadAsStringAsync();
                return _mapper.Map<TokenContext>(JsonConvert.DeserializeObject<TRAuthTokenDTO>(resp));
            }
            else
                throw new NotAuthorizedException($"Email {context.Email} - Rewards not Authorized");
        }
    }
}
