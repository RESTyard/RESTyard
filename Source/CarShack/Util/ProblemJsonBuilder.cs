using RESTyard.WebApi.Extensions.ErrorHandling;

namespace CarShack.Util
{
    public static class ProblemJsonBuilder
    {
        public static ProblemJson CreateEntityNotFound()
        {
            var problem = new ProblemJson
            {
                Title = "Entity not found",
                Detail = string.Empty,
                ProblemType = "CarShack.EntityNotFound",
                StatusCode = 404
            };

            return problem;
        }

        public static ProblemJson CreateBadParameters()
        {
            var problem = new ProblemJson
            {
                Title = "Bad Parameters",
                Detail = "Review parameter schema.",
                ProblemType = "CarShack.BadParameters",
                StatusCode = 400
            };

            return problem;
        }
    }
}