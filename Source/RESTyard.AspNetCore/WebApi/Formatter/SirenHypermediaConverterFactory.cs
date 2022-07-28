using System;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.Formatter
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