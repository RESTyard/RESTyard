using System;

namespace Bluehands.Hypermedia.Client.Exceptions
{
    /// <summary>
    /// If the request is not successful (http status code != 2xx) this exception is thrown.
    /// </summary>
    public class RequestNotSuccessfulException : Exception
    {
        public RequestNotSuccessfulException(string title, int statusCode, Exception inner = null) : base(title, inner)
        {
            this.Title = title;
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localisation.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The HTTP status code set by the origin server for this occurrence of the problem.
        /// </summary>
        public int StatusCode { get; set; }
    }
}