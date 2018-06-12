namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The used resource has changed.
    /// </summary>
    public class ResourceVersionConflict : HypermediaClientException
    {
        public ResourceVersionConflict(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}