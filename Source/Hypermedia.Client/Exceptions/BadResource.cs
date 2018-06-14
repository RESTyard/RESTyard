namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The resource resolved is not of the expected type or is not readable.
    /// </summary>
    public class BadResource : HypermediaProblemException
    {
        public BadResource(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}