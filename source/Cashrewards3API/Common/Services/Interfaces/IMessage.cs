using Cashrewards3API.Common.Events;
using Cashrewards3API.Enum;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services.Interfaces
{
    public interface IMessage
    {
        Task UpdatedPremiumMemberProperty(MemberPremiumUpdateProperty message);

        Task UpdatePremiumMemberEvent(MemberPremiumUpdateEvent message, PremiumStatusEnum premiumStatus);

        Task MemberFirstClickEvent(MemberFirstClickEvent message);
    }
}