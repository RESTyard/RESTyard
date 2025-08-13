using System.Net;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.ErrorHandling;

namespace CarShack.Util
{
    public static class ProblemJsonBuilder
    {
        
        public static ProblemDetails UnexpectedError(string message)
        {
            var problem = new ProblemDetails()
            {
                Title = "Unexpected error",
                Detail = message,
                Status = (int)HttpStatusCode.InternalServerError,
            };

            return problem;
        }
        
        public static ProblemDetails CreateEntityNotFound()
        {
            var problem = new ProblemDetails()
            {
                Title = "Entity not found",
                Detail = string.Empty,
                Type = "CarShack.EntityNotFound",
                Status = (int)HttpStatusCode.NotFound,
            };

            return problem;
        }

        public static ProblemDetails CreateBadParameters()
        {
            var problem = new ProblemDetails
            {
                Title = "Bad Parameters",
                Detail = "Review parameter schema.",
                Type = "CarShack.BadParameters",
                Status = (int)HttpStatusCode.BadRequest,
            };

            return problem;
        }
    }
}