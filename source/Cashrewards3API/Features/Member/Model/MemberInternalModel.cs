using System;

namespace Cashrewards3API.Features.Member.Model
{
    public class MemberInternalModel
    {
        public Guid MemberNewId { get; set; }

        public int MemberId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PostCode { get; set; }

        public string AccessCode { get; set; }

        public string Mobile { get; set; }

        public int Status { get; set; }

        public string UserPassword { get; set; }

        public string SaltKey { get; set; }
    }
}
