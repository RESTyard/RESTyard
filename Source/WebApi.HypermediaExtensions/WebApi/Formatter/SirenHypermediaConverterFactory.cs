using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    class SirenHypermediaConverterFactory : ISirenHypermediaConverterFactory
    {
        private readonly IQueryStringBuilder queryStringBuilder;
        private readonly ISirenConverterConfiguration sirenConverterConfiguration;

        public SirenHypermediaConverterFactory(IQueryStringBuilder queryStringBuilder, ISirenConverterConfiguration sirenConverterConfiguration = null)
        {
            this.queryStringBuilder = queryStringBuilder;
            this.sirenConverterConfiguration = sirenConverterConfiguration;
        }

        public IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver)
        {
            return new SirenConverter(hypermediaRouteResolver, queryStringBuilder, sirenConverterConfiguration);
        }
    }
}