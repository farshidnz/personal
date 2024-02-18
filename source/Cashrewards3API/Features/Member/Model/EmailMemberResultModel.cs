using System;

namespace Cashrewards3API.Features.Member.Model
{
    public class EmailMemberDto : MemberDto
    {
        public DateTime? LastTouchPoint { get; set; }
        public int? Status { get; set; }
    }
}