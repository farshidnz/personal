using Cashrewards3API.Features.Person.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public class PremiumMembership
    {
        public int PremiumClientId { get; set; }
        public int PremiumMemberId { get; set; }
        public bool IsCurrentlyActive { get; set; }
    }

    public interface IPremiumService
    {
        Task<PremiumMembership> GetPremiumMembership(int baseClientId, string cognitoId);

        int? GetPremiumClientId(int baseClientId);
    }

    public class PremiumService : IPremiumService
    {
        private readonly IPerson _personService;

        public PremiumService(IPerson personService)
        {
            _personService = personService;
        }

        public async Task<PremiumMembership> GetPremiumMembership(int baseClientId, string cognitoId)
        {
            if (string.IsNullOrEmpty(cognitoId))
            {
                return null;
            }

            var premiumClientId = GetPremiumClientId(baseClientId);
            if (!premiumClientId.HasValue)
            {
                return null;
            }

            var person = await _personService.GetPerson(cognitoId);
            if (person == null || person.PremiumStatus == Enum.PremiumStatusEnum.NotEnrolled)
            {
                return null;
            }

            var premiumMember = person.Members.FirstOrDefault(m => m.ClientId == premiumClientId);

            return new PremiumMembership
            {
                PremiumClientId = premiumClientId.Value,
                PremiumMemberId = premiumMember?.MemberId ?? 0,
                IsCurrentlyActive = person.PremiumStatus == Enum.PremiumStatusEnum.Enrolled
            };
        }

        private Dictionary<int, int> PremiumOffers = new Dictionary<int, int>
        {
            [Constants.Clients.CashRewards] = Constants.Clients.Blue
        };

        public int? GetPremiumClientId(int baseClientId)
        {
            return PremiumOffers.TryGetValue(baseClientId, out var premiumClientId) ? premiumClientId : null;
        }
    }
}
