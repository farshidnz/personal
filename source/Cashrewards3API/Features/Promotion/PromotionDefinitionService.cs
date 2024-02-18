using AutoMapper;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Promotion.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Promotion
{
    public interface IPromotionDefinitionService
    {
        Task<PromotionDefinition> GetPromotionDefinition(string slug);
    }

    public class PromotionDefinitionService : IPromotionDefinitionService
    {
        private readonly string _s3BucketName;
        private readonly IMapper _mapper;
        private readonly IAwsS3Service _awsS3Service;
        private readonly IStrapiService _strapiService;

        public PromotionDefinitionService(
            IConfiguration configuration,
            IMapper mapper,
            IAwsS3Service awsS3Service,
            IStrapiService strapiService)
        {
            _s3BucketName = configuration["Config:PromotionBucketName"];
            _mapper = mapper;
            _awsS3Service = awsS3Service;
            _strapiService = strapiService;
        }

        public async Task<PromotionDefinition> GetPromotionDefinition(string slug)
        {
            var promotionDefinition = await GetPromotionDefinitionFromStrapi(slug);
            if (promotionDefinition == null)
            {
                promotionDefinition = await GetPromotionDefinitionFromS3(slug);
            }

            return promotionDefinition;
        }

        private async Task<PromotionDefinition> GetPromotionDefinitionFromS3(string slug)
        {
            var promotion = await _awsS3Service.ReadAmazonS3Data($"{slug}.json", _s3BucketName);

            if (string.IsNullOrEmpty(promotion))
                throw new NotFoundException($"{slug} promtion definion not found");

            var promotionDefinition = JsonConvert.DeserializeObject<PromotionDefinition>(promotion,
                new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
            if (promotionDefinition == null)
                throw new NotFoundException($"{slug} invalid promtion found");

            return promotionDefinition;
        }

        private async Task<PromotionDefinition> GetPromotionDefinitionFromStrapi(string slug)
        {
            var strapiPromotion = await _strapiService.GetCampaign(slug);
            if (strapiPromotion != null)
            {
                return _mapper.Map<PromotionDefinition>(strapiPromotion);
            }

            return null;
        }
    }
}
