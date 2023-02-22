using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// The parameters for an Action are not correct.
    /// </summary>
    public class BadParameters : HypermediaProblemException
    {
        public BadParameters(ProblemDetails problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}