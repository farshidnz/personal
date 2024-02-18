using Cashrewards3API.Common.Dto;
using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantTierSummaryBase
    {
        private string _clientCommissionString;

        public decimal Commission { get; set; }

        public decimal ClientComm { get; set; }

        public decimal MemberComm { get; set; }

        public decimal ClientCommission => Math.Round(Commission * (ClientComm / 100) * (MemberComm / 100),2);

        public int TierCommTypeId { get; set; }

        public int TierTypeId { get; set; }

        public int ClientProgramTypeId { get; set; }

        public decimal Rate { get; set; }

        public int TierCount { get; set; }

        public bool? IsFlatRate { get; set; }

        public string RewardName { get; set; }

        public string ClientCommissionString => _clientCommissionString ?? (_clientCommissionString = GetCommissionString(ClientProgramTypeId, TierCommTypeId, ClientCommission, Rate, IsFlatRate, TierTypeId, RewardName));

        public static string GetCommissionString(int clientProgramTypeId, int tierCommTypeId, decimal clientCommission, decimal rate, bool? isFlatRate, int tierTypeId, string rewardName, bool alwaysUse2DecimalPlaces = false)
        {
            if (clientProgramTypeId != (int)ClientProgramTypeEnum.PointsProgram)
            {
                return GetCashrewardsCommissionString(tierCommTypeId, clientCommission, tierTypeId, isFlatRate, rewardName, alwaysUse2DecimalPlaces).Trim();
            }

            return GetPointsCommissionString(tierCommTypeId, clientCommission, rate, isFlatRate, rewardName);
        }

        public static string GetTierCommissionString(decimal clientCommission, int tierCommTypeId, bool alwaysUse2DecimalPlaces = false)
        {
           if (tierCommTypeId == (int)TierCommTypeEnum.Dollar)
            {
                alwaysUse2DecimalPlaces = alwaysUse2DecimalPlaces || clientCommission.RoundToTwoDecimalPlaces() % 1 != 0;
                var commission = clientCommission.RoundToTwoDecimalPlaces().ToString(alwaysUse2DecimalPlaces ? "F" : "G29");
                return $"${commission}";
            }
            else 
            {
                var commission = clientCommission.RoundToTwoDecimalPlaces().ToString(alwaysUse2DecimalPlaces ? "F" : "G29");
                return $"{commission}%";
            }
        }

        private static string GetPointsCommissionString(int tierCommTypeId, decimal clientCommission, decimal rate, bool? isFlatRate, string rewardName)
        {
            var sb = new StringBuilder();

            var isDollarType = tierCommTypeId == (int)TierCommTypeEnum.Dollar;
            var commissionPts = isDollarType ? clientCommission * rate : clientCommission / 100 * rate;
            commissionPts = commissionPts.RoundToTwoDecimalPlaces();

            if (isFlatRate ?? true)
            {
                sb.Append($"{commissionPts.ToString("G29")} {rewardName}");
            }
            else
            {
                sb.Append($"Up to {commissionPts.ToString("G29")} {rewardName}");
            }

            if (!isDollarType)
            {
                sb.Append("/$");
            }

            return sb.ToString();
        }

        private static string GetCashrewardsCommissionString(int tierCommTypeId, decimal clientCommission, int tierTypeId, bool? isFlatRate, string rewardName, bool alwaysUse2DecimalPlaces)
        {
            var sb = new StringBuilder();
            BuildCashrewardsCommisionString_Pre(sb, tierTypeId, isFlatRate);

            if (tierCommTypeId == (int)TierCommTypeEnum.Dollar)
            {
                alwaysUse2DecimalPlaces = alwaysUse2DecimalPlaces || clientCommission.RoundToTwoDecimalPlaces() % 1 != 0;
                sb.Append($"${clientCommission.RoundToTwoDecimalPlaces().ToString(alwaysUse2DecimalPlaces ? "F" : "G29")}");
            }
            else
            {
                sb.Append($"{clientCommission.RoundToTwoDecimalPlaces().ToString(alwaysUse2DecimalPlaces ? "F" : "G29")}%");
            }

            BuildCashrewardsCommisionString_Post(sb, tierTypeId, rewardName);

            return sb.ToString();
        }

        private static void BuildCashrewardsCommisionString_Pre(StringBuilder sb, int tierTypeId, bool? isFlatRate)
        {
            if (tierTypeId == (int)TierTypeEnum.Discount)
            {
                sb.Append("Save ");
                return;
            }

            if (tierTypeId == (int)TierTypeEnum.MaxDiscount)
            {
                sb.Append("Save up to ");
                return;
            }

            if (isFlatRate ?? false)
            {
                sb.Append(string.Empty);
            }
            else
            {
                sb.Append("Up to ");
            }
        }

        private static void BuildCashrewardsCommisionString_Post(StringBuilder sb, int tierTypeId, string rewardName)
        {
            if (tierTypeId == (int)TierTypeEnum.Discount)
            {
                return;
            }
            if (tierTypeId == (int)TierTypeEnum.MaxDiscount)
            {
                return;
            }

            sb.Append($" {rewardName}");
        }
    }
}
