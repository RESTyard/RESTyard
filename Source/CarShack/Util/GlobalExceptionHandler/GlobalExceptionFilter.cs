using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using RESTyard.AspNetCore.ErrorHandling;
using RESTyard.AspNetCore.Exceptions;

namespace CarShack.Util.GlobalExceptionHandler
{
    public class GlobalExceptionFilter : IExceptionFilter, IDisposable
    {
        private readonly ILogger logger;

        public GlobalExceptionFilter(ILoggerFactory logger)
        {
            if (logger != null)
            {
                this.logger = logger.CreateLogger("Global Exception Filter");
            }
        }

        public void OnException(ExceptionContext context)
        {
            TypeSwitch.Do(context.Exception,
                TypeSwitch.Case<HypermediaException>(() => this.HandleHypermediaException(context)),
                TypeSwitch.Case<HypermediaFormatterException>(() => this.HandleHypermediaException(context)),
                TypeSwitch.Case<UnauthorizedAccessException>(() => this.HandleUnauthorizedAccessException(context)),
                TypeSwitch.Default(() => GenericResponse(context))
            );

            if (this.logger != null)
            {
                this.logger.LogError("GlobalExceptionFilter", context.Exception);
            }
        }

        private void HandleUnauthorizedAccessException(ExceptionContext context)
        {
            var response = new ProblemDetails()
            {
                Title = "Not authorized.",
                Type = "WebApi.HypermediaExtensions.NotAuthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Extensions =
                {
                    { "ExceptionDetail", context.Exception.ToString() },  
                },
            };

            CreateResultObject(context, response);
        }

        private void HandleHypermediaException(ExceptionContext context)
        {
            var response = new ProblemDetails()
            {
                Title = "Hypermedia error.",
                Type = "WebApi.HypermediaExtensions.HyperrmediaError",
                Status = (int)HttpStatusCode.InternalServerError,
                Extensions =
                {
                    { "ExceptionDetail", context.Exception.ToString() },
                },
            };

            CreateResultObject(context, response);
        }

        private static void GenericResponse(ExceptionContext context)
        {
            var response = new ProblemDetails() {
                Title = "Sorry, something went wrong.",
                Type = "WebApi.HypermediaExtensions.InternalError",
                Status = (int)HttpStatusCode.InternalServerError,
                Extensions =
                {
                    { "ExceptionDetail", context.Exception.ToString() },
                },
            };

            CreateResultObject(context, response);
        }

        private static void CreateResultObject(ExceptionContext context, ProblemDetails response)
        {
            context.Result = new ObjectResult(response)
            {
                StatusCode = response.Status,
                DeclaredType = response.GetType()
            };
        }

        public void Dispose()
        {
        }
    }
}
