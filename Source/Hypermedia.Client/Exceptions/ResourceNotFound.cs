namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Resolving a link failed because tjhe ressource does not exist.
    /// </summary>
    public class ResourceNotFound : HypermediaProblemException
    {
        public ResourceNotFound(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}