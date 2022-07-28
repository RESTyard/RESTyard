using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// The the client is not authentificated.
    /// </summary>
    public class Unauthorized : HypermediaProblemException
    {
        public Unauthorized(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}