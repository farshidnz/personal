using Cashrewards3API.Features.Member.Model;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Member.Interface
{
    public interface IMemberRegistrationService
    {
        Task<ClientMemberResultModel> CreateBlueMemberFromCashRewardsMember(string cognitoId);
    }
}