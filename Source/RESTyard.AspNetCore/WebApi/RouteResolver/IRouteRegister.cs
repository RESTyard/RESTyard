using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public interface IRouteRegister
    {
        bool TryGetRoute(Type lookupType, out RouteInfo routeInfo);

        void AddActionRoute(Type hypermediaActionType, string routeName, string httpMethod, string acceptableMediaType);

        void AddHypermediaObjectRoute(Type hypermediaObjectType, string routeName, string httpMethod);

        void AddParameterTypeRoute(Type iHypermediaActionParameter, string routeName, string httpMethod);

        void AddRouteKeyProducer(Type attributeRouteType, IKeyProducer keyProducer);

        bool TryGetKeyProducer(Type getType, [NotNullWhen(true)] out IKeyProducer? keyProducer);
    }
}