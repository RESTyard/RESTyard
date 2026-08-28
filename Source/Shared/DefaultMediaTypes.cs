using System;

namespace RESTyard.MediaTypes
{
    public static class DefaultMediaTypes
    {
        [Obsolete("Use MediaTypeNames.Application.Json")]
        public const string ApplicationJson = "application/json";
        
        public const string JsonSchema = "application/schema+json";

        public const string Siren = "application/vnd.siren+json";

        [Obsolete("Use MediaTypeNames.Application.ProblemJson")]
        public const string ProblemJson = "application/problem+json";
        
        [Obsolete("Use MediaTypeNames.Multipart.FormData")]
        public const string MultipartFormData = "multipart/form-data";
        
        [Obsolete("Use MediaTypeNames.Application.Octet")]
        public const string OctetStream = "application/octet-stream";
    }
}