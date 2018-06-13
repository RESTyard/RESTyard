namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Error of the client.
    /// </summary>
    public class ClientError : HypermediaProblemException
    {
        public ClientError(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}