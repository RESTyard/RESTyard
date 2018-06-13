namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The the client is not authentificated.
    /// </summary>
    public class Unauthorized : HypermediaProblemException
    {
        public Unauthorized(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}