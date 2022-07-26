using System;
using RESTyard.WebApi.Extensions.Query;
using RESTyard.WebApi.Extensions.WebApi.ExtensionMethods;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi.Formatter
{
    internal class SirenHypermediaConverterFactory : ISirenHypermediaConverterFactory
    {
        private readonly IQueryStringBuilder queryStringBuilder;
        private readonly HypermediaExtensionsOptions hypermediaExtensionsOptions;

        public SirenHypermediaConverterFactory(IQueryStringBuilder queryStringBuilder, HypermediaExtensionsOptions hypermediaExtensionsOptions)
        {
            this.queryStringBuilder = queryStringBuilder;
            this.hypermediaExtensionsOptions = hypermediaExtensionsOptions;
        }

        public IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver)
        {
            return new SirenConverter(hypermediaRouteResolver, queryStringBuilder, hypermediaExtensionsOptions.HypermediaConverterConfiguration);
        }
    }
}