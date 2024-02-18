using System;
using System.Collections.Generic;

namespace Cashrewards3API.Common.Dto
{
    public class CognitoMemberDto
    {
        public string AccessCode { get; set; }

        public int? CampaignId { get; set; }

        public Guid CognitoId { get; set; }

        public string CognitoPoolId { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OriginalSource { get; set; }

        public List<MemberInfoDto> MemberInfo { get; set; } = new List<MemberInfoDto>();
    }

    public class MemberInfoDto
    {
        public int MemberId { get; set; }
        public int ClientId { get; set; }
        public Guid MemberNewId { get; set; }
    }
}