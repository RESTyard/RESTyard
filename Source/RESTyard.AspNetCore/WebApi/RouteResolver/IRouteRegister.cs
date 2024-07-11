using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public interface IRouteRegister
    {
        bool TryGetRoute(Type lookupType, out RouteInfo routeInfo);

        void AddActionRoute(Type hypermediaActionType, string routeName, HttpMethod httpMethod, string acceptableMediaType);

        void AddHypermediaObjectRoute(Type hypermediaObjectType, string routeName, HttpMethod httpMethod);

        void AddParameterTypeRoute(Type iHypermediaActionParameter, string routeName, HttpMethod httpMethod);

        void AddRouteKeyProducer(Type attributeRouteType, IKeyProducer keyProducer);

        bool TryGetKeyProducer(Type getType, [NotNullWhen(true)] out IKeyProducer? keyProducer);
    }
}