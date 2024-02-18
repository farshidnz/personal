using System;

namespace Cashrewards3API.Features.Member.Model
{
    public class CognitoMemberModel
    {
        public string CognitoId { get; set; }
        public int MemberId { get; set; }
        public string CognitoPoolId { get; set; }
        public Guid MemberNewId { get; set; }
        public bool Status { get; set; }
        public int? PersonId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}