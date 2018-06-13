namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// The parameters for an Action are not correct.
    /// </summary>
    public class BadParameters : HypermediaProblemException
    {
        public BadParameters(string Title, string ProblemType, string Detail)
            : base(Title, ProblemType, Detail)
        {
        }
    }
}