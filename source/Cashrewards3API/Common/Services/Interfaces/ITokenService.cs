using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services.Model;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenContext> GetToken(AuthnRequestContext context);
    }
}