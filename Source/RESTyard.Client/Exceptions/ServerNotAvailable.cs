using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// Server is not available.
    /// </summary>
    public class ServerNotAvailable : HypermediaProblemException
    {
        public ServerNotAvailable(ProblemDetails problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}