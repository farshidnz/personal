using Cashrewards3API.Common;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Cashrewards3API.Security
{
    public class ClientCredentialsTokenRequirement : IAuthorizationRequirement
    {
        public int ClientId { get; }

        public ClientCredentialsTokenRequirement(int clientId)
        {
            ClientId = clientId;
        }
    }

    public class ClientCredentialsTokenAuthorizationHandler : AuthorizationHandler<ClientCredentialsTokenRequirement>
    {
        private readonly IRequestContext _requestContext;

        public ClientCredentialsTokenAuthorizationHandler(IRequestContext requestContext)
        {
            _requestContext = requestContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClientCredentialsTokenRequirement requirement)
        {
#if DEBUG
            context.Succeed(requirement);
            return Task.CompletedTask;
#endif

            int clientId = _requestContext.GetClientIdFromDynamoDbAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (clientId == requirement.ClientId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}