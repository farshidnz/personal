using Cashrewards3API.Common;
using Cashrewards3API.Common.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Security
{
    public class AccessTokenRequirement : IAuthorizationRequirement
    {
        public int ClientId { get; }

        public AccessTokenRequirement(int clientId)
        {
            ClientId = clientId;
        }
    }

    public class AccessTokenAuthorizationHandler : AuthorizationHandler<AccessTokenRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IClientService _clientService;

        private const string TOKEN_CLIENT_ID_CLAIM_NAME = "client_id";
        private const string TOKEN_USERNAME_CLAIM_NAME = "username";
        private const string X_ACCESS_TOKEN_HEADER = "x-access-token";
        private readonly ITokenValidation _tokenValidation;

        public AccessTokenAuthorizationHandler(IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IClientService clientService,
        IMemoryCache memoryCache,
        ITokenValidation tokenValidation)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientService = clientService;
            _tokenValidation = tokenValidation;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessTokenRequirement requirement)
        {
#if DEBUG
            context.Succeed(requirement);
            return Task.CompletedTask;
#endif

            var accessToken = _httpContextAccessor.HttpContext.Request.Headers[X_ACCESS_TOKEN_HEADER];

            try
            {
                var jwtToken = _tokenValidation.ValidateToken(accessToken);

                if (jwtToken != null
                    && ValidateRequirementsFromToken(jwtToken, requirement))
                    context.Succeed(requirement);
            }
            catch (System.Exception ex)
            {
                context.Fail();
            }
            return Task.CompletedTask;
        }

        private bool ValidateRequirementsFromToken(JwtSecurityToken jwtToken, AccessTokenRequirement requirement)
        {
            if (jwtToken.Claims.FirstOrDefault(c => c.Type == TOKEN_USERNAME_CLAIM_NAME).Value == null)
                return false;

            var cognitoClientId = jwtToken.Claims.FirstOrDefault(c => c.Type == TOKEN_CLIENT_ID_CLAIM_NAME).Value;
            if (!String.IsNullOrEmpty(cognitoClientId))
            {
                string clientId = _clientService.GetPartner(cognitoClientId).ConfigureAwait(false).GetAwaiter().GetResult();
                if (clientId != requirement.ClientId.ToString())
                    return false;
            }

            return true;
        }
    }
}