using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Utils.Extensions;
using Cashrewards3API.Features.ReferAFriend.Model;
using Dapper;

namespace Cashrewards3API.Features.ReferAFriend
{
    public interface IRafService
    {
        Task<RafResultModel> GetRafBonus(string newMemberId);
    }
    public class RafService : IRafService
    {
    
        private readonly ShopgoDBContext shopgoDBContext;
        private readonly CommonConfig commonConfig;

        public RafService(ShopgoDBContext shopgoDBContext,
            CommonConfig commonConfig)
        {
            this.shopgoDBContext = shopgoDBContext;
            this.commonConfig = commonConfig;
        }

        public async Task<RafResultModel> GetRafBonus(string newMemberId)
        {
            return await GetRafBonusFromDbAsync(newMemberId);
        }

        private RafResultModel ConvertToRafResultModel(RafModel rafModel) 
            => new RafResultModel()
            {
                HasQualifiedTransactions =  rafModel.QualifiedTransactionId.HasValue  ? true : false,
                Status = rafModel.FriendBuyConversionId.HasValue ? true : false,
                Promotion = rafModel.FriendBuyConversionId.HasValue ? 
                    new RafPromotion()
                    {
                        BonusType = Constants.RefferAFriend.DefaultBonusType,
                        BonusValue = rafModel.BonusValue.GetValueOrDefault().RoundToTwoDecimalPlaces(),
                        TransactionStatus = rafModel.TransactionStatusId.GetDescription(),
                        TransactionType = rafModel.TransactionTypeId.GetDescription(),
                        TransactionTypeId = (int)rafModel.TransactionTypeId,

                        Rules = new RafRules()
                        {
                            AccessCode = new RafAccessCode()
                            {
                                Equals = rafModel.AccessCode,
                                Required = true
                            },
                            PurchaseWindow = new RafPurchaseWindow()
                            {
                                Max = Constants.RefferAFriend.PurchnaseWindowMax,
                                Required = true
                            },
                            SaleValue = new RafSaleValue()
                            {
                                Min= Constants.RefferAFriend.SaleValueMin,
                                Required = true
                            }
                                
                        }
                    } 
                    : null
        };
        

        private async Task<RafResultModel> GetRafBonusFromDbAsync(string newMemberId)
        {
            string queryString = $@"SELECT        m.MemberNewId, m.AccessCode, TT.MemberCommissionValueAud AS BonusValue, FBC.AmountRequirement PurchaseSaleValue, 
                                                  T.TransactionStatusId, T.TransactionTypeId, TR.TransactionId AS QualifiedTransactionId, FC.FriendBuyConversionId 
                                        FROM          dbo.Member AS M LEFT OUTER JOIN  
						                         dbo.FriendBuyConversion AS FC ON FC.MateMemberId = M.MemberId LEFT OUTER JOIN
						                         dbo.FriendBuyCampaign AS FBC ON FBC.FriendBuyCampaignId = FC.FriendBuyCampaignId LEFT OUTER JOIN
						                         dbo.[Transaction] AS T ON FC.MateTransReference = T.TransactionReference AND T.Status = 1 LEFT OUTER JOIN
                                                 dbo.TransactionTier AS TT ON T.TransactionId = TT.TransactionId 
                                                 LEFT OUTER JOIN dbo.TransactionReferMateEligible TR on TR.FriendBuyConversionId = FC.FriendBuyConversionId
                                                 WHERE  m.AccessCode = @accessCode
                                                 AND MemberNewId = @memberNewId ";

            using var conn = shopgoDBContext.CreateReadOnlyConnection();
            var rafBounsResponse = await conn.QueryAsync<RafModel>(queryString,
                new
                {
                    memberNewId = newMemberId,
                    accessCode = Constants.RefferAFriend.DefaultAccessCode
                });

            var bounsResponse = rafBounsResponse as RafModel[] ?? rafBounsResponse.ToArray();
            var rafBonusModel = bounsResponse.FirstOrDefault();

            if (rafBonusModel != null)
                return ConvertToRafResultModel(bounsResponse.FirstOrDefault());

            return null;
        }
    }
}
