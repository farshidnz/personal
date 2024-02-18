using System;

namespace Cashrewards3API.Features.Member.Model
{
    public class MemberDto
    {
        public int MemberId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid MemberNewId { get; set; }

        public string AccessCode { get; set; }

        public string Email { get; set; }

        public int ClientId { get; set; }

        public DateTime? DateJoined { get; set; }

        public string PostCode { get; set; }

        public string Mobile { get; set; }

        public int? CampaignId { get; set; }

        public int Source { get; set; }

        public string OriginationSource { get; set; }

        public int PremiumStatus { get; set; }

        public bool IsValidated { get; set; }

        public bool ReceiveNewsLetter { get; set; }
    }
}