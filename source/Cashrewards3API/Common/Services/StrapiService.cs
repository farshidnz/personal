using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Features.Promotion.Model;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public interface IStrapiService
    {
        Task<StrapiCampaign> GetCampaign(string slug);
    }

    public class StrapiService : IStrapiService
    {
        private readonly HttpClient _httpClient;
        private readonly bool _useStrapiv4 = false;

        public StrapiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _useStrapiv4 = configuration["UseStrapiV4"]?.ToLowerInvariant() == "true";

            var strapiVersion = _useStrapiv4 ? "strapiv4" : "strapi";
            _httpClient = httpClientFactory.CreateClient(strapiVersion);
        }

        public async Task<StrapiCampaign> GetCampaign(string slug)
        {
            var uri = _useStrapiv4 ? $"campaigns?filters[slug][$eq]={slug}" : $"campaigns?_where[slug]={slug}";
            var response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                if (_useStrapiv4)
                {
                    var campaign = await response.Content.ReadAsAsync<StrapiCampaign>();
                    return (campaign?.Data?.Count ?? 0) == 0 ? null : campaign;
                }
                else
                {
                    var campaigns = await response.Content.ReadAsAsync<IEnumerable<StrapiCampaign>>();
                    return campaigns?.FirstOrDefault();
                }
            }
            else
            {
                throw new HttpRequestException($"Strapi API error status: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
    }
}
