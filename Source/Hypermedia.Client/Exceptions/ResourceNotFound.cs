using System;

namespace Bluehands.Hypermedia.Client.Exceptions
{
    /// <summary>
    /// Resolving a link failed because tjhe ressource does not exist.
    /// </summary>
    public class ResourceNotFound : HypermediaProblemException
    {
        public ResourceNotFound(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}