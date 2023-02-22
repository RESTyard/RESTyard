using System;
using System.Net;

namespace RESTyard.Client.Exceptions
{
    /// <summary>
    /// If a request error occurs in the client he tries to read a ProblemJson from the body and throws this exception.
    /// </summary>
    public class HypermediaProblemException : RequestNotSuccessfulException
    {
        public HypermediaProblemException(ProblemDetails problemDetails, Exception inner = null)
            : base(problemDetails.Title, problemDetails.Status, inner)
        {
            this.ProblemDetails = problemDetails;
        }

        public ProblemDetails ProblemDetails { get; }
    }
}


