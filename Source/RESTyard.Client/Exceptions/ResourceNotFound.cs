using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// Resolving a link failed because the resource does not exist.
    /// </summary>
    public class ResourceNotFound : HypermediaProblemException
    {
        public ResourceNotFound(ProblemDetails problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}