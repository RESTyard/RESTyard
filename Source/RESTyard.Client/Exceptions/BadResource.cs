using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// The resource resolved is not of the expected type or is not readable.
    /// </summary>
    public class BadResource : HypermediaProblemException
    {
        public BadResource(ProblemDetails problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}