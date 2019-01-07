using System;

namespace Bluehands.Hypermedia.Client.Exceptions
{
    /// <summary>
    /// The parameters for an Action are not correct.
    /// </summary>
    public class BadParameters : HypermediaProblemException
    {
        public BadParameters(ProblemDescription problemDescription, Exception inner = null)
            : base(problemDescription, inner)
        {
        }
    }
}