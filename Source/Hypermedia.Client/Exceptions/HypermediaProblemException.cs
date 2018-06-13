using Hypermedia.Client.Resolver;

namespace Hypermedia.Client.Exceptions
{
    /// <summary>
    /// If a error occurs in the client throws an Exception of this base type.
    /// Errors in the client are not tied to http errors, because protokoll migth be changed. This his a abstraction of ProblemJson.
    /// </summary>
    public class HypermediaProblemException : HypermediaClientException
    {
        public HypermediaProblemException(string title, string problemType, string detail) : base(title, detail)
        {
            this.ProblemType = problemType;
        }

        public HypermediaProblemException(ProblemDescription problemDescription) : base(problemDescription.Title, problemDescription.Detail)
        {
            this.ProblemType = problemDescription.ProblemType;
        }

        /// <summary>
        /// An absolute URI that identifies the problem type. When dereferenced, it SHOULD provide human-readable documentation for the problem type(e.g., using HTML).
        /// </summary>
        public string ProblemType { get; }
    }
}


