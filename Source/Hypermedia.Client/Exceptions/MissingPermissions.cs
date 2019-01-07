using System;

namespace Bluehands.Hypermedia.Client.Exceptions
{
    /// <summary>
    /// The requested operation is not allowed for the current user.
    /// </summary>
    public class MissingPermissions : HypermediaProblemException
    {
        public MissingPermissions(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}