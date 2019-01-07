using System;

namespace Bluehands.Hypermedia.Client.Exceptions
{
    /// <summary>
    /// Server is not available.
    /// </summary>
    public class ServerNotAvailable : HypermediaProblemException
    {
        public ServerNotAvailable(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}