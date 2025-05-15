using System;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{

    public class RouteKeyFactory : IRouteKeyFactory
    {
        readonly IRouteRegister routeRegister;

        public RouteKeyFactory(IRouteRegister routeRegister)
        {
            this.routeRegister = routeRegister;
        }

        public object GetHypermediaRouteKeys(IHypermediaObject hypermediaObject)
        {
            if (!this.routeRegister.TryGetKeyProducer(hypermediaObject.GetType(), out var keyProducer))
            {
                return new { };
            }

            return keyProducer.CreateFromHypermediaObject(hypermediaObject);
        }

        public object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference)
        {
            if (!this.routeRegister.TryGetKeyProducer(reference.GetHypermediaType(), out var keyProducer))
            {
                return new { };
            }

            var key = reference.GetKey(keyProducer);
            if (key == null)
            {
                return new { };
            }

            return key;
        }

        public object GetActionRouteKeys(HypermediaActionBase action, IHypermediaObject actionHostObject)
        {
            if (!this.routeRegister.TryGetKeyProducer(action.GetType(), out var keyProducer) 
                && !this.routeRegister.TryGetKeyProducer(actionHostObject.GetType(), out keyProducer))
            {
                return new { };
            }

            return keyProducer.CreateFromHypermediaObject(actionHostObject);
        }
    }
}