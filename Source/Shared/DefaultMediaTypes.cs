using System;

namespace RESTyard.MediaTypes
{
    public static class DefaultMediaTypes
    {
        public const string ApplicationJson = "application/json";
        
        public const string JsonSchema = "application/schema+json";

        public const string Siren = "application/vnd.siren+json";

        public const string ProblemJson = "application/problem+json";

        /// <summary>
        /// Media type for the authored API guide (manual) served by the api guide endpoint.
        /// The body is raw Markdown; the vendor subtype and <c>+markdown</c> suffix are
        /// RESTyard-internal conventions (not IANA-registered).
        /// </summary>
        public const string ApiGuide = "text/vnd.restyard.api-guide+markdown";
        
        public const string MultipartFormData = "multipart/form-data";
        
        public const string OctetStream = "application/octet-stream";
    }
}