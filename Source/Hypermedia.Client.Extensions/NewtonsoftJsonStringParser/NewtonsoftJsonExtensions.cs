using System;
using Bluehands.Hypermedia.Client;
using Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson;
using Newtonsoft.Json;

namespace Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson
{
    public static class NewtonsoftJsonExtensions
    {
        public static HypermediaClientBuilder WithNewtonsoftJsonStringParser(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomStringParser(() => new NewtonsoftJsonStringParser());
        }

        public static HypermediaClientBuilder WithNewtonsoftJsonObjectSerializer(this HypermediaClientBuilder builder, Formatting formatting = Formatting.None)
        {
            return builder.WithCustomParameterSerializer(() => new NewtonsoftJsonObjectParameterSerializer(formatting));
        }

        public static HypermediaClientBuilder WithSingleNewtonsoftJsonParameterSerializer(this HypermediaClientBuilder builder, Formatting formatting = Formatting.None)
        {
            return builder.WithCustomParameterSerializer(() => new SingleNewtonsoftJsonObjectParameterSerializer(formatting));
        }

        public static HypermediaClientBuilder WithNewtonsoftJsonProblemReader(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomProblemStringReader(() => new NewtonsoftJsonProblemStringReader());
        }
    }
}
