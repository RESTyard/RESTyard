namespace Hypermedia.Client.Exceptions
{
    using System;

    /// <summary>
    /// If a error occurs in the client throws an Exception of this base type.
    /// Errors in the client are not tied to http errors, because protokoll migth be changed. This his a abstraction of ProblemJson.
    /// </summary>
    public abstract class HypermediaClientException : Exception
    {
        public HypermediaClientException(string title, Uri problemType, string detail)
        {
            this.Title = title;
            this.ProblemType = problemType;
            this.Detail = detail;
        }

        /// <summary>
        /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localisation.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// An absolute URI that identifies the problem type. When dereferenced, it SHOULD provide human-readable documentation for the problem type(e.g., using HTML).
        /// </summary>
        public Uri ProblemType { get; }

        /// <summary>
        /// An human readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; }
    }
}


