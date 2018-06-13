namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// Server is not available.
    /// </summary>
    public class ServerNotAvailable : HypermediaProblemException
    {
        public ServerNotAvailable(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}