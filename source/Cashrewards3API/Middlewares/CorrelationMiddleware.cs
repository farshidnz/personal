using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Common.Context;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Cashrewards3API.Middlewares
{
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.Headers.TryGetValue("x-request-id", out var correlationIds);
            var correlationId = correlationIds.FirstOrDefault() ?? Guid.NewGuid().ToString();
            CorrelationContext.SetCorrelationId(correlationId);
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next.Invoke(context);
            }
        }
    }
}
