using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using WebApi.HypermediaExtensions.ErrorHandling;
using WebApi.HypermediaExtensions.Exceptions;

namespace CarShack.Util.GloblaExceptionHandler
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
            var response = new ExceptionProblemJson(context.Exception)
            {
                Title = "Not authorized.",
                ProblemType = "WebApi.HypermediaExtensions.NotAuthorized",
                StatusCode = (int)HttpStatusCode.Unauthorized
            };

            CreateResultObject(context, response);
        }

        private void HandleHypermediaException(ExceptionContext context)
        {
            var response = new ExceptionProblemJson(context.Exception)
            {
                Title = "Hypermedia error.",
                ProblemType = "WebApi.HypermediaExtensions.HyperrmediaError",
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            CreateResultObject(context, response);
        }

        private static void GenericResponse(ExceptionContext context)
        {
            var response = new ExceptionProblemJson(context.Exception) {
                Title = "Sorry, something went wrong.",
                ProblemType = "WebApi.HypermediaExtensions.InternalError",
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            CreateResultObject(context, response);
        }

        private static void CreateResultObject(ExceptionContext context, ExceptionProblemJson response)
        {
            context.Result = new ObjectResult(response)
                             {
                                 StatusCode = response.StatusCode,
                                 DeclaredType = response.GetType()
                             };
        }

        public void Dispose()
        {
        }
    }
}
