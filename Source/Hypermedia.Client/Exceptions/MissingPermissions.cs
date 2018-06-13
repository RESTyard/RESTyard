namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The requested operation is not allowed for the current user.
    /// </summary>
    public class MissingPermissions : HypermediaProblemException
    {
        public MissingPermissions(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}