namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Resolving a link failed because tjhe ressource does not exist.
    /// </summary>
    public class ResourceNotFound : HypermediaClientException
    {
        public ResourceNotFound(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}