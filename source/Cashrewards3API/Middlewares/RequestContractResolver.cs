using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;
using System;

namespace Cashrewards3API.Middlewares
{
    public class RequestContractResolver : IContractResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IContractResolver _camelCase = new CamelCasePropertyNamesContractResolver();
        private readonly IContractResolver _default = new DefaultContractResolver();

        public RequestContractResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public JsonContract ResolveContract(Type type)
        {
            if (_httpContextAccessor.HttpContext.Request.Headers["AcceptCase"] == "camel")
            {
                return _camelCase.ResolveContract(type);
            }

            return _default.ResolveContract(type);
        }
    }
}
