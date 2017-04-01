using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public interface ISirenHypermediaConverterFactory
    {
        IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver);
    }
}