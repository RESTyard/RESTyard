namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The used resource has changed.
    /// </summary>
    public class ResourceVersionConflict : HypermediaProblemException
    {
        public ResourceVersionConflict(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}