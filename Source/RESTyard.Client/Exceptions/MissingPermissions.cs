using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// The requested operation is not allowed for the current user.
    /// </summary>
    public class MissingPermissions : HypermediaProblemException
    {
        public MissingPermissions(ProblemDetails problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}