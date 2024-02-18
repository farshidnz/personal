using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Banners.Interface;
using Cashrewards3API.Features.Banners.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Banners.Service
{
    public class BannerService : IBanner
    {
        private readonly IMapper _mapper;
        private readonly IRedisUtil _redisUtil;
        private readonly ICacheKey _cacheKey;
        private readonly CacheConfig _cacheConfig;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IReadOnlyRepository _readOnlyRepository;

        public BannerService(IMapper mapper, IRedisUtil redisUtil,
                               ICacheKey cacheKey,
                               CacheConfig cacheConfig,
                               IDateTimeProvider dateTimeProvider,
                               IReadOnlyRepository readOnlyRepository)
        {
            _mapper = mapper;
            _redisUtil = redisUtil;
            _cacheKey = cacheKey;
            _cacheConfig = cacheConfig;
            _dateTimeProvider = dateTimeProvider;
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<IEnumerable<Banner>> GetBannersFromClientId(int clientId)
        {
            string key = _cacheKey.GetBannersForClientIds(clientId);
            var banners = await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                    () => GetBannersFromDb(clientId), _cacheConfig.CategoryDataExpiry);

            return banners.OrderBy(b => b.Position).ToList();
        }

        /// <summary>
        /// Gets the banners from database.
        /// </summary>
        /// <param name="clientsId">The clients identifier.</param>
        /// <returns></returns>
        private async Task<IEnumerable<Banner>> GetBannersFromDb(int clientsId)
        {
            var banners = new Dictionary<int, Banner>();

            var query = @"SELECT [Id]
                          ,[Name]
                          ,[Status]
                          ,[StartDate]
                          ,[EndDate]
                          ,[DesktopHtml]
                          ,[MobileHtml]
                          ,[DesktopLink]
                          ,[MobileLink]
                          ,[DesktopImageUrl]
                          ,[MobileImageUrl]
                          ,[Position]
                          ,[MobileAppImageUrl]
                          ,[MobileAppLink]
                          ,bc.[ClientId]

                      FROM [dbo].[Banner] B WITH (NOLOCK)
                      LEFT JOIN [dbo].BannerClient BC WITH (NOLOCK) ON BC.BannerId = B.Id
                      WHERE Status=1 AND BC.IsActive = 1 AND StartDate <= @dateTime AND EndDate > @dateTime AND BC.ClientId = @clientId";

            var allBanners = await _readOnlyRepository.QueryAsync<Banner, int, Banner>(query, (pd, pp) =>
                   {
                       Banner banner = new();
                       if (!banners.TryGetValue(pd.Id, out banner))
                       {
                           banners.Add(pd.Id, banner = pd);
                       }

                       banner.Clients.Add(pp);
                       return banner;
                   }, param: new { clientId = clientsId, dateTime = _dateTimeProvider.Now },
                    splitOn: "ClientId");

            return allBanners;
        }
    }
}