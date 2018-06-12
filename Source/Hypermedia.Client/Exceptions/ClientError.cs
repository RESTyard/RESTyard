namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Error of the client.
    /// </summary>
    public class ClientError : HypermediaClientException
    {
        public ClientError(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}