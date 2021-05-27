using System;
using System.Text.Json;
using Bluehands.Hypermedia.Client;

namespace Bluehands.Hypermedia.Client.Extensions.SystemTextJsonStringParser
{
    public static class SystemTextJsonExtensions
    {
        public static HypermediaClientBuilder WithSystemTextJsonStringParser(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomStringParser(() => new SystemTextJsonStringParser());
        }

        public static HypermediaClientBuilder WithSystemTextJsonObjectSerializer(this HypermediaClientBuilder builder, JsonSerializerOptions options = null)
        {
            return builder.WithCustomParameterSerializer(() => new SystemTextJsonObjectParameterSerializer(options));
        }

        public static HypermediaClientBuilder WithSingleSystemTextJsonObjectSerializer(this HypermediaClientBuilder builder, JsonWriterOptions options = default)
        {
            return builder.WithCustomParameterSerializer(() => new SingleSystemTextJsonObjectParameterSerializer(options));
        }

        public static HypermediaClientBuilder WithSystemTextJsonProblemReader(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomProblemStringReader(() => new SystemTextJsonProblemStringReader());
        }
    }
}
