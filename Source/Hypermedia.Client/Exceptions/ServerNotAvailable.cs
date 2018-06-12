namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Server is not available.
    /// </summary>
    public class ServerNotAvailable : HypermediaClientException
    {
        public ServerNotAvailable(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}