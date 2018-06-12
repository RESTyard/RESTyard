namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The the client is not authorized.
    /// </summary>
    public class Unauthorized : HypermediaClientException
    {
        public Unauthorized(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}