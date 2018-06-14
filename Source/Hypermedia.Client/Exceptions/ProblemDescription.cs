namespace Hypermedia.Client.Exceptions
{
    public class ProblemDescription
    {
        /// <summary>
        /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localisation.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// An absolute URI that identifies the problem type. When dereferenced, it SHOULD provide human-readable documentation for the problem type(e.g., using HTML).
        /// </summary>
        public string ProblemType { get; set; }

        /// <summary>
        /// An human readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// The HTTP status code set by the origin server for this occurrence of the problem.
        /// </summary>
        public int StatusCode { get; set; }
    }
}