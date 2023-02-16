using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// Unhandled error on the server.
    /// </summary>
    public class ServerError : HypermediaProblemException
    {
        public ServerError(ProblemDetails problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}