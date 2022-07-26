using System;

namespace RESTyard.WebApi.Extensions.WebApi.RouteResolver
{
    public interface IRouteRegister
    {
        bool TryGetRoute(Type lookupType, out RouteInfo routeInfo);

        void AddActionRoute(Type hypermediaActionType, string routeName, HttpMethod httpMethod, string acceptableMediaType);

        void AddHypermediaObjectRoute(Type hypermediaObjectType, string routeName, HttpMethod httpMethod);

        void AddParameterTypeRoute(Type iHypermediaActionParameter, string routeName, HttpMethod httpMethod);

        void AddRouteKeyProducer(Type attributeRouteType, IKeyProducer keyProducer);

        bool TryGetKeyProducer(Type getType, out IKeyProducer keyProducer);
    }
}