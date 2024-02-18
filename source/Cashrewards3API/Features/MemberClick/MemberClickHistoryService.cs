using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.MemberClick.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.MemberClick
{
    public interface IMemberClickHistoryService
    {
        Task<PagedList<MemberClickHistoryResultModel>> GetMemberClicksHistory(MemberClickHistoryRequestInfoModel model);
        
    }
    public class MemberClickHistoryService : IMemberClickHistoryService
    {
        private readonly ILogger<MemberClickHistoryService> logger;
        private readonly ShopgoDBContext shopgoDBContext;
        private readonly INetworkExtension _networkExtension;

        public MemberClickHistoryService(ILogger<MemberClickHistoryService> logger,
            ShopgoDBContext shopgoDBContext, 
            INetworkExtension networkExtension)
        {
            this.logger = logger;
            this.shopgoDBContext = shopgoDBContext;
            _networkExtension = networkExtension;
        }


        public async Task<PagedList<MemberClickHistoryResultModel>> GetMemberClicksHistory(MemberClickHistoryRequestInfoModel model)
        {
            var memberClicks = await GetMemberClicksHistoryFromDb(model);
            var memberClicksResultModels  = memberClicks.Data.Select(click => ConvertToMemberClickHistoryResultModel(click)).ToList();
            return new PagedList<MemberClickHistoryResultModel>(
                                            memberClicks.TotalCount,
                                            memberClicksResultModels.Count,
                                            memberClicksResultModels);
        }

        private MemberClickHistoryResultModel ConvertToMemberClickHistoryResultModel(MemberClickHistoryModel memberClickHistoryModel)
        {
            var src = memberClickHistoryModel;
            return new MemberClickHistoryResultModel
            {
                ClickId = src.ClickId,
                MerchantId = src.MerchantId,
                ClickCount = src.ClickCount,
                DateCreated = src.DateCreated,
                MerchantName = src.MerchantName,
                HyphenatedString = src.HyphenatedString,
                MemberId = src.MemberId,
                NetworkId = src.NetworkId,
                FromMobileApp = _networkExtension.IsInMobileSpecificNetwork(src.NetworkId),
                DateCreatedUtc = src.DateCreatedUtc
            };
        }

        #region db queries

        private async Task<PagedList<MemberClickHistoryModel>> GetMemberClicksHistoryFromDb(MemberClickHistoryRequestInfoModel request)
        {
            string SQLQuery = @"DECLARE @totalCount int;
                                
                                SELECT @totalCount = count(1)
                                FROM MEMBERCLICKS MC
                                INNER JOIN MERCHANT M ON MC.MERCHANTID= M.MERCHANTID
                                WHERE MEMBERID = @MemberId;

                                SELECT MC.ClickId AS ClickId, MC.MERCHANTID AS MerchantId,0 As ClickCount,DATECREATED As DateCreated,M.MERCHANTNAME As MerchantName,
                                       M.HYPHENATEDSTRING AS HyphenatedString,MemberId As MemberId,ISNULL(MC.AdBlockerEnabled,0) AS AdBlockEnabled, M.NetworkId, 
                                       MC.DATECREATEDUTC As DateCreatedUtc, @totalCount As TotalCount
                                FROM MEMBERCLICKS MC
                                INNER JOIN MERCHANT M ON MC.MERCHANTID= M.MERCHANTID
                                WHERE MEMBERID = @memberId
                                ORDER BY DATECREATED DESC
                                OFFSET @offset ROWS
                                FETCH NEXT @limit ROWS ONLY;
                                ";

            using var conn = shopgoDBContext.CreateReadOnlyConnection();
            var memberClickHistory = await conn.QueryMultipleAsync(SQLQuery, 
                new { 
                     memberId = request.MemberId,
                     offset = request.Offset,
                     limit = request.Limit
                });
            var clickhistory = (await memberClickHistory.ReadAsync<MemberClickHistoryModel>()).ToList();
            int totalCount = clickhistory.Count > 0 ? clickhistory.FirstOrDefault().TotalCount : 0;
            return new PagedList<MemberClickHistoryModel>(totalCount, clickhistory.Count, clickhistory);
        }
        #endregion
    }
}
