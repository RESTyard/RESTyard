using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
    using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

    public class RouteKeyFactory : IRouteKeyFactory
    {
        private readonly IRouteRegister routeRegister;

        public RouteKeyFactory(IRouteRegister routeRegister)
        {
            this.routeRegister = routeRegister;
        }

        public object GetHypermediaRouteKeys(HypermediaObject hypermediaObject)
        {
            var keyProducer = this.routeRegister.GetKeyProducer(hypermediaObject.GetType());
            if (keyProducer == null)
            {
                return new { };
            }

            return keyProducer.CreateFromHypermediaObject(hypermediaObject);
        }

        public object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference)
        {
            var keyProducer = this.routeRegister.GetKeyProducer(reference.GetHypermediaType());
            if (keyProducer == null)
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

        public object GetActionRouteKeys(HypermediaActionBase action, HypermediaObject actionHostObject)
        {
            var keyProducer = this.routeRegister.GetKeyProducer(action.GetType());
            if (keyProducer == null)
            {
                return new { };
            }

            return keyProducer.CreateFromHypermediaObject(actionHostObject);
        }
    }
}