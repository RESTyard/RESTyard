namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Unhandled error on the server.
    /// </summary>
    public class ServerError : HypermediaProblemException
    {
        public ServerError(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}