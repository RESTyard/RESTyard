namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    using global::WebApi.HypermediaExtensions.Query;
    using global::WebApi.HypermediaExtensions.WebApi.ExtensionMethods;
    using global::WebApi.HypermediaExtensions.WebApi.RouteResolver;

    internal class SirenHypermediaConverterFactory : ISirenHypermediaConverterFactory
    {
        private readonly IQueryStringBuilder queryStringBuilder;
        private readonly HypermediaConverterConfiguration hypermediaConverterConfiguration;

        public SirenHypermediaConverterFactory(IQueryStringBuilder queryStringBuilder, HypermediaConverterConfiguration hypermediaConverterConfiguration = null)
        {
            this.queryStringBuilder = queryStringBuilder;
            this.hypermediaConverterConfiguration = hypermediaConverterConfiguration;
        }

        public IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver)
        {
            return new SirenConverter(hypermediaRouteResolver, this.queryStringBuilder, this.hypermediaConverterConfiguration);
        }
    }
}