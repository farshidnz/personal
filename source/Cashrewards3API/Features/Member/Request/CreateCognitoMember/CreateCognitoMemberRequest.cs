using Cashrewards3API.Enum;
using System;

namespace Cashrewards3API.Features.Member.Request
{
    public class CreateCognitoMemberRequest
    {
        public string AccessCode { get; set; }

        public int? CampaignId { get; set; }

        public Guid CognitoId { get; set; }

        public string CognitoPoolId { get; set; }

        public ClientsEnum ClientId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OriginalSource { get; set; }
        public string Source { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string PostCode { get; set; }

        public string FacebookUsername { get; set; }
    }
}