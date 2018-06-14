using Hypermedia.Client.Resolver;

namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// If a request error occurs in the client he trys to read a ProblemJson from the body and throws this exception.
    /// </summary>
    public class HypermediaProblemException : Exception
    {
        public HypermediaProblemException(string title, string problemType, string detail, int statusCode, Exception inner = null) : base(title, inner)
        {
            this.Title = title;
            this.Detail = detail;
            this.ProblemType = problemType;
            this.StatusCode = statusCode;
        }

        public HypermediaProblemException(ProblemDescription problemDescription, Exception inner = null)
            : this(problemDescription.Title, problemDescription.ProblemType, problemDescription.Detail, problemDescription.StatusCode, inner)
        {
        }

        /// <summary>
        /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localisation.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// An human readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// An absolute URI that identifies the problem type. When dereferenced, it SHOULD provide human-readable documentation for the problem type(e.g., using HTML).
        /// </summary>
        public string ProblemType { get; }

        /// <summary>
        /// The HTTP status code set by the origin server for this occurrence of the problem.
        /// </summary>
        public int StatusCode { get; set; }
    }
}


