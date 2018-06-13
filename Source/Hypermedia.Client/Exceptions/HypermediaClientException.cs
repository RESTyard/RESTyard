namespace Hypermedia.Client.Exceptions
{
    using System;

    public class HypermediaClientException : Exception
    {
        public HypermediaClientException(string title, string detail)
        {
            this.Title = title;
            this.Detail = detail;
        }

        /// <summary>
        /// A short, human-readable summary of the problem type. It SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localisation.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// An human readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; }
    }
}