using System;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public interface IRouteRegister
    {
        bool TryGetRoute(Type lookupType, out string routeName);

        void AddActionRoute(Type hypermediaActionType, string routeName);

        void AddHypermediaObjectRoute(Type hypermediaObjectType, string routeName);

        void AddParameterTypeRoute(Type iHypermediaActionParameter, string routeName);

        void AddRouteKeyProducer(Type attributeRouteType, IKeyProducer keyProducer);

        IKeyProducer GetKeyProducer(Type getType);
    }
}