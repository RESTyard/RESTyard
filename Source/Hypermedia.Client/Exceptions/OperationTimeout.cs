namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The requested operation timed out.
    /// </summary>
    public class OperationTimeout : HypermediaClientException
    {
        public OperationTimeout(string Title, Uri ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}