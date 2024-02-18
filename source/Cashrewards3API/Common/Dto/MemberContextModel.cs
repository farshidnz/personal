using System;
using System.Collections.Generic;

namespace Cashrewards3API.Common.Dto
{
    public class MemberContextModel
    {
        public int MemberId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid MemberNewId { get; set; }

        public string AccessCode { get; set; }

        public string Email { get; set; }

        public DateTime? DateJoined { get; set; }

        public int ClientId { get; set; }

        public string Mobile { get; set; }

        public bool ReceiveNewsLetter { get; set; }

        public bool IsValidated { get; set; }
    }
}