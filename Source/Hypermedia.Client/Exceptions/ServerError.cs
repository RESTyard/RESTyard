namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Unhandled error on the server.
    /// </summary>
    public class ServerError : HypermediaClientException
    {
        public ServerError(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}