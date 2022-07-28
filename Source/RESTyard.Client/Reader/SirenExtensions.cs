using System;
using RESTyard.Client.Builder;

namespace RESTyard.Client.Reader
{
    public static class SirenExtensions
    {
        /// <summary>
        /// Use the SIREN format (https://github.com/kevinswiber/siren) to read incoming data
        /// </summary>
        /// <returns></returns>
        public static IHypermediaResolverBuilder WithSirenHypermediaReader(this IHypermediaResolverBuilder builder)
        {
            return builder.WithCustomHypermediaReader((register, parser) => new SirenHypermediaReader(register, parser));
        }
    }
}