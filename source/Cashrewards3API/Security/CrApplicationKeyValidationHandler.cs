using AutoMapper.Internal;
using Cashrewards3API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Security
{
    public class CrApplicationKeyValidationRequirement : IAuthorizationRequirement
    {
    }

    public class CrApplicationKeyValidationHandler : AuthorizationHandler<CrApplicationKeyValidationRequirement>
    {
        private IHttpContextAccessor _httpContextAccessor = null;
        private readonly ICrApplicationKeyValidationService _crApplicationKeyValidationService;

        public CrApplicationKeyValidationHandler(IHttpContextAccessor httpContextAccessor, ICrApplicationKeyValidationService crApplicationKeyValidationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _crApplicationKeyValidationService = crApplicationKeyValidationService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CrApplicationKeyValidationRequirement requirement)
        {
#if DEBUG
            context.Succeed(requirement);
            return Task.CompletedTask;
#endif

            string CR_APPLICATION_HEADER = "Cr-Application-Key";

            HttpContext httpContext = _httpContextAccessor.HttpContext;

            if (!httpContext.Request.Headers.ContainsKey(CR_APPLICATION_HEADER))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (!_crApplicationKeyValidationService.IsValid(httpContext.Request.Headers.GetOrDefault(CR_APPLICATION_HEADER).FirstOrDefault()))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}