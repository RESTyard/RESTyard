using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    internal class SirenHypermediaConverterFactory : ISirenHypermediaConverterFactory
    {
        private readonly IQueryStringBuilder queryStringBuilder;
        private readonly ISirenConverterConfiguration sirenConverterConfiguration;

        public SirenHypermediaConverterFactory(IQueryStringBuilder queryStringBuilder, ISirenConverterConfiguration sirenConverterConfiguration = null)
        {
            this.queryStringBuilder = queryStringBuilder;
            this.sirenConverterConfiguration = sirenConverterConfiguration;
        }

        public IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver, ApplicationModel applicationModel)
        {
            return new SirenConverter(hypermediaRouteResolver, queryStringBuilder, applicationModel, sirenConverterConfiguration);
        }
    }
}