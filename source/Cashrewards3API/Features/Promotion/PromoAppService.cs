using Cashrewards3API.Features.Promotion.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Promotion
{
    public interface IPromoAppService
    {
        Task<PromoDetailsModel> GetPromotionDetails(string code);
    }

    public class PromoAppService : IPromoAppService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public PromoAppService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<PromoDetailsModel> GetPromotionDetails(string code)
        {
            var client = _httpClientFactory.CreateClient("promoapp");

            var response = await client.GetAsync($"{_configuration["Config:PromoApp:CouponValidationEndpoint"]}{code}");
            var promoJson = await response.Content.ReadAsStringAsync();
            var promo = JsonConvert.DeserializeObject<PromoDetailsModel>(promoJson);

            return promo;
        }
    }
}
