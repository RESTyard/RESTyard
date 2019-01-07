using System;

namespace Bluehands.Hypermedia.Client.Exceptions
{
    /// <summary>
    /// Unhandled error on the server.
    /// </summary>
    public class ServerError : HypermediaProblemException
    {
        public ServerError(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}