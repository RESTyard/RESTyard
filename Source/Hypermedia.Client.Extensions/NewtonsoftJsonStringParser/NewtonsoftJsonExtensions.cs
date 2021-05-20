using System;
using Bluehands.Hypermedia.Client;
using Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson;

namespace Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson
{
    public static class NewtonsoftJsonExtensions
    {
        public static HypermediaClientBuilder WithNewtonsoftJsonStringParser(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomStringParser(() => new NewtonsoftJsonStringParser());
        }

        public static HypermediaClientBuilder WithJsonObjectSerializer(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomParameterSerializer(() => new NewtonsoftJsonObjectParameterSerializer());
        }

        public static HypermediaClientBuilder WithSingleJsonParameterSerializer(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomParameterSerializer(() => new SingleNewtonsoftJsonObjectParameterSerializer());
        }

        public static HypermediaClientBuilder WithNewtonsoftJsonProblemReader(this HypermediaClientBuilder builder)
        {
            return builder.WithCustomProblemStringReader(() => new NewtonsoftJsonProblemStringReader());
        }
    }
}
