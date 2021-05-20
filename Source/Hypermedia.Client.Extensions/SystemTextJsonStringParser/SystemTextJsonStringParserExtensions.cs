using System;
using Bluehands.Hypermedia.Client;

namespace Bluehands.Hypermedia.Client.Extensions.SystemTextJsonStringParser
{
    public static class SystemTextJsonStringParserExtensions
    {
        public static HypermediaClientBuilder WithSystemTextJsonStringParser(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomStringParser(() => new SystemTextJsonStringParser());
        }
    }
}
