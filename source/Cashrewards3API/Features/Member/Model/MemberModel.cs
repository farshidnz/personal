using System;
using System.Collections.Generic;
using Cashrewards3API.Common.Dto;

namespace Cashrewards3API.Features.Member.Model
{
    public class MemberModel
    {
        public Guid CognitoId { get; set; }

        public string CognitoIdString
        {
            get { return CognitoId.ToString("N"); }
            set { CognitoId = new Guid(value); }
        }

        public string CognitoPoolId { get; set; }

        public DateTime? DateJoined { get; set; }

        public int MemberId { get; set; }

        public int? CampaignId { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid MemberNewId { get; set; }

        public string AccessCode { get; set; }

        public string Email { get; set; }

        public int ClientId { get; set; }

        public string Mobile { get; set; }

        public bool ReceiveNewsLetter { get; set; }

        public bool IsValidated { get; set; }

        public string Password { get; set; }

        public string PostCode { get; set; }
        public string OriginationSource { get; set; }
        public string Source { get; set; }
        public int? PersonId { get; set; }
        public int PremiumStatus { get; set; }
        public List<MemberInfoDto> MemberInfo { get; set; } = new List<MemberInfoDto>();
        public string FacebookUsername { get; set; }

        public int Status { get; set; }
    }
}