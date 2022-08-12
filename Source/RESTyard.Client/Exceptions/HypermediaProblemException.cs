using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// If a request error occurs in the client he trys to read a ProblemJson from the body and throws this exception.
    /// </summary>
    public class HypermediaProblemException : RequestNotSuccessfulException
    {
        public HypermediaProblemException(string title, string problemType, string detail, int statusCode, Exception inner = null)
            : base(title, statusCode, inner)
        {
            this.Detail = detail;
            this.ProblemType = problemType;
        }

        public HypermediaProblemException(ProblemDescription problemDescription, Exception inner = null)
            : this(problemDescription.Title, problemDescription.Type, problemDescription.Detail, problemDescription.Status, inner)
        {
        }

        /// <summary>
        /// An human readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// An absolute URI that identifies the problem type. When dereferenced, it SHOULD provide human-readable documentation for the problem type(e.g., using HTML).
        /// </summary>
        public string ProblemType { get; }
    }
}


