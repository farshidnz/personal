using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services.Interfaces
{
    public interface ITokenValidation
    {
        JwtSecurityToken ValidateToken(string token);
    }
}