using Cashrewards3API.Enum;
using System.Text.Json.Serialization;

namespace Cashrewards3API.Features.Person.Request.UpdatePerson
{
    public class UpdatePersonRequest
    {
        [JsonIgnore]
        public string CognitoId { get; set; }

        public PremiumStatusEnum PremiumStatus { get; set; }
    }
}