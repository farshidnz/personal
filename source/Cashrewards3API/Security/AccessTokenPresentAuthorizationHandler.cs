using Cashrewards3API.Common.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace Cashrewards3API.Security
{

    public class AccessTokenPresentRequirement : IAuthorizationRequirement
    {
        public AccessTokenPresentRequirement()
        {
        }
    }

    public class AccessTokenPresentAuthorizationHandler : AuthorizationHandler<AccessTokenPresentRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ITokenValidation _tokenValidation;

        public AccessTokenPresentAuthorizationHandler(IHttpContextAccessor httpContextAccessor,
        ITokenValidation tokenValidation)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenValidation = tokenValidation;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessTokenPresentRequirement requirement)
        {
            //#if DEBUG
            //            context.Succeed(requirement);
            //            return Task.CompletedTask;
            //#endif

            if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                var accessToken = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization];
                accessToken = accessToken.ToString().Replace("Bearer", string.Empty).Trim();

                _tokenValidation.ValidateToken(accessToken);
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}