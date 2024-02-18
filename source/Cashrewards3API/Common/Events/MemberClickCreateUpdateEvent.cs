using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Events
{
    public class MemberClickCreateUpdateEvent
    {
        public int? CampaignId { get; set; }

        public string CashbackOffer { get; set; }

        public string TrackingId { get; set; }

        public int? ItemId { get; set; }

        public string ItemType { get; set; }

        public int MemberId { get; set; }

        public int MerchantId { get; set; }

        public string MerchantName { get; set; }

        public int NetworkId { get; set; }

        public string NetworkName { get; set; }

        public int TenantId { get; set; }

        public string IPAddress { get; set; }

        public DateTime DateCreated { get; set; }

        public string UserAgent { get; set; }

        public string RedirectionLinkUsed { get; set; }

        public bool Update { get; set; }

        public string EventName
        {
            get
            {
                return "MemberClickCreateUpdateEvent";
            }
        }
    }
}
