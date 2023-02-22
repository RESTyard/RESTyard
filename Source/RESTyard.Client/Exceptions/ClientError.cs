using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// Error of the client.
    /// </summary>
    public class ClientError : HypermediaProblemException
    {
        public ClientError(ProblemDetails problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}