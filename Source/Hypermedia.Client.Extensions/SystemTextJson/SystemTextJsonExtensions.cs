using System.Text.Json;

namespace Bluehands.Hypermedia.Client.Extensions.SystemTextJson
{
    public static class SystemTextJsonExtensions
    {
        public static HypermediaClientBuilder WithSystemTextJsonStringParser(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomStringParser(() => new SystemTextJsonStringParser());
        }

        public static HypermediaClientBuilder WithSystemTextJsonObjectParameterSerializer(this HypermediaClientBuilder builder, JsonSerializerOptions options = null)
        {
            return builder.WithCustomParameterSerializer(() => new SystemTextJsonObjectParameterSerializer(options));
        }

        public static HypermediaClientBuilder WithSingleSystemTextJsonObjectParameterSerializer(this HypermediaClientBuilder builder, JsonWriterOptions options = default)
        {
            return builder.WithCustomParameterSerializer(() => new SingleSystemTextJsonObjectParameterSerializer(options));
        }

        public static HypermediaClientBuilder WithSystemTextJsonProblemReader(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomProblemStringReader(() => new SystemTextJsonProblemStringReader());
        }
    }
}
