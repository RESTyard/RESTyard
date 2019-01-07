using System;

namespace Bluehands.Hypermedia.Client.Exceptions
{
    /// <summary>
    /// The used resource has changed.
    /// </summary>
    public class ResourceVersionConflict : HypermediaProblemException
    {
        public ResourceVersionConflict(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}