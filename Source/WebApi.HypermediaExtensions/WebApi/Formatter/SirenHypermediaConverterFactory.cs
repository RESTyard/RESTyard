namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    using Query;
    using ExtensionMethods;
    using RouteResolver;

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