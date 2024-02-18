using Cashrewards3API.Common.Context;
using Cashrewards3API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ArgumentException = Cashrewards3API.Exceptions.ArgumentException;

namespace Cashrewards3API.Middlewares
{
    // TODO: add data layer exceptions
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string errorGuid = CorrelationContext.GetCorrelationId() ?? Guid.NewGuid().ToString();

            var code = HttpStatusCode.InternalServerError;
            
            if (exception is BadRequestException)
                code = HttpStatusCode.BadRequest;

            if (exception is NotFoundException)
                code = HttpStatusCode.NotFound;

            if (exception is ArgumentException || exception is ArgumentOutOfRangeException)
                code = HttpStatusCode.BadRequest;
            
            if (exception is NotAuthorizedException)
                code = HttpStatusCode.Unauthorized;
           
            string jsonResultFrontEnd = JsonSerializer.Serialize(new ErrorResponse
            {
                Message = exception.Message.ToString(),
                ErrorId = errorGuid
            });

            JsonResult jsonResultLog = new JsonResult(new
            {
                error = exception.Message,
                errorId = errorGuid,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException,               
            }
            );
            jsonResultLog.StatusCode = (int)code;
            if(exception is not NotAuthorizedException)
                _logger.LogError(Newtonsoft.Json.JsonConvert.SerializeObject(jsonResultLog));

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(jsonResultFrontEnd);
        }
    }
}