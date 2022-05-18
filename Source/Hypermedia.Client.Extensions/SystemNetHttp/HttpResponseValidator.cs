using System;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public readonly struct HttpResponseValidator
    {
        public string ETag { get; }

        public DateTimeOffset? LastModified { get; }

        public HttpResponseValidator(
            string etag,
            DateTimeOffset? lastModified)
        {
            ETag = etag;
            LastModified = lastModified;
        }
    }
}