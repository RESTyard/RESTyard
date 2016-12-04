using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes
{
    public interface IHaveRouteKeyProducer
    {
        IRouteKeyProducer RouteKeyProducer { get; }
    }
}