using Cashrewards3API.Common.Events;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services.Interfaces
{
    public interface IEvent
    {
        Task UpdatePremiumEvent(MemberPremiumUpdateEvent eventMessage);
    }
}