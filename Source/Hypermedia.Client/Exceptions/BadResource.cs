namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The resource resolved is not of the expected type or is not readable.
    /// </summary>
    public class BadResource : HypermediaProblemException
    {
        public BadResource(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}