namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The parameters for an Action are not correct.
    /// </summary>
    public class BadParameters : HypermediaClientException
    {
        public BadParameters(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}