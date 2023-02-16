using System;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// If the request is not successful this exception is thrown.
    /// </summary>
    public class RequestNotSuccessfulException : Exception
    {
        public RequestNotSuccessfulException(string title, int? status, Exception inner = null) : base(title, inner)
        {
            this.Status = status;
        }

        /// <summary>
        /// The status code set by the origin server for this occurrence of the problem.
        /// </summary>
        public int? Status { get; set; }
    }
}