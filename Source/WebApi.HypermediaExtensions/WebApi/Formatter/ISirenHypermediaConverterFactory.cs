using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    public interface ISirenHypermediaConverterFactory
    {
        IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver, ApplicationModel applicationModel);
    }
}