using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi.AttributedRoutes
{
    public interface IHaveRouteKeyProducer
    {
        IKeyProducer RouteKeyProducer { get; }
    }
}