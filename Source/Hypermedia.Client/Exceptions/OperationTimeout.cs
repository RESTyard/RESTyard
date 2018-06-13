namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The requested operation timed out.
    /// </summary>
    public class OperationTimeout : HypermediaProblemException
    {
        public OperationTimeout(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}