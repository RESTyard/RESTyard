using System;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    public interface IRouteRegister
    {
        string GetRoute(Type lookupType);

        void AddActionRoute(Type hypermediaActionType, string routeName);

        void AddHypermediaObjectRoute(Type hypermediaObjectType, string routeName);

        void AddParameterTypeRoute(Type iHypermediaActionParameter, string routeName);

        void AddRouteKeyProducer(Type attributeRouteType, IKeyProducer keyProducer);

        bool TryGetKeyProducer(Type getType, out IKeyProducer keyProducer);
        bool TryGetRoute(Type lookupType, out string routeName);
    }
}