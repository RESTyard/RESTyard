namespace Hypermedia.Client.Exceptions
{
    using System;

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