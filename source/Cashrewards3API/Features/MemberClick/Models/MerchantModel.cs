using System;
using Cashrewards3API.Common.Utils.Extensions;
using Cashrewards3API.Enum;
using System.Text;

namespace Cashrewards3API.Features.MemberClick
{
    public class MerchantModel
    {
        public int MerchantId { get; set; }

        public int NetworkId { get; set; }

        public string MerchantName { get; set; }

        public string RegularImageUrl { get; set; }

        public string TrackingLink { get; set; }

        public MobileAppTrackingTypeEnum MobileAppTrackingType { get; set; }

        public string WebsiteUrl { get; set; }

        public int ClientProgramTypeId { get; set; }

        public int TierCommTypeId { get; set; }

        public decimal Commission { get; set; }

        public decimal ClientComm { get; set; }

        public decimal MemberComm { get; set; }


        public decimal ClientCommission => Math.Round( Commission * (ClientComm / 100) * (MemberComm / 100), 2);

        public decimal Rate { get; set; }

        public bool? IsFlatRate { get; set; }

        public int TierTypeId { get; set; }

        public string RewardName { get; set; }

        public int ClientId { get; set; }

        public bool IsPaused { get; set; }

        private string _clientCommissionString;
        public string ClientCommissionString => _clientCommissionString ?? (_clientCommissionString = GetCommissionString(ClientProgramTypeId, TierCommTypeId, ClientCommission, Rate, IsFlatRate, TierTypeId, RewardName));

        public static string GetCommissionString(int clientProgramTypeId, int tierCommTypeId, decimal clientCommission, decimal rate, bool? isFlatRate, int tierTypeId, string rewardName)
        {
            if (clientProgramTypeId != (int)ClientProgramTypeEnum.PointsProgram)
            {
                return GetCashrewardsCommissionString(tierCommTypeId, clientCommission, tierTypeId, isFlatRate, rewardName);
            }

            return GetPointsCommissionString(tierCommTypeId, clientCommission, rate, isFlatRate, rewardName);
        }
        public int MobileTrackingNetwork { get; set; }

        private static string GetPointsCommissionString(int tierCommTypeId, decimal clientCommission, decimal rate, bool? isFlatRate, string rewardName)
        {
            var sb = new StringBuilder();

            var isDollarType = tierCommTypeId == (int)TierCommTypeEnum.Dollar;
            var commissionPts = isDollarType ? clientCommission * rate : clientCommission / 100 * rate;
            commissionPts = commissionPts.RoundToTwoDecimalPlaces();

            if (isFlatRate ?? true)
            {
                sb.Append($"{commissionPts} {rewardName}");
            }
            else
            {
                sb.Append($"Up to {commissionPts} {rewardName}");
            }

            if (!isDollarType)
            {
                sb.Append("/$");
            }

            return sb.ToString();
        }

        private static string GetCashrewardsCommissionString(int tierCommTypeId, decimal clientCommission, int tierTypeId, bool? isFlatRate, string rewardName)
        {
            var sb = new StringBuilder();
            BuildCashrewardsCommisionString_Pre(sb, tierTypeId, isFlatRate);

            if (tierCommTypeId == (int)TierCommTypeEnum.Dollar)
            {
                sb.Append($"${clientCommission.ToString("F2")}");
            }
            else
            {
                sb.Append($"{clientCommission.ToString("F2")}%");
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
