using System.Reflection;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.WebApi.RouteResolver
{
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

            return keyProducer.GetKey(hypermediaObject);
        }

        public object GetHypermediaRouteKeys(HypermediaObjectReferenceBase reference)
        {
            var type = reference.GetType();
            if (typeof(HypermediaObjectKeyReference).IsAssignableFrom(type))
            {
                return this.GetHypermediaRouteKeys(reference as HypermediaObjectKeyReference);
            }
            if (type == typeof(HypermediaObjectReference))
            {
                return this.GetHypermediaRouteKeys(reference as HypermediaObjectReference);
            }
            // queries must not have a key

            return new { };
        }

        public object GetHypermediaRouteKeys(HypermediaObjectReference reference)
        {
            return this.GetHypermediaRouteKeys(reference.Resolve());
        }

        public object GetHypermediaRouteKeys(HypermediaObjectKeyReference reference)
        {
            var referenceKey = reference.GetKey();
            if (referenceKey == null)
            {
                return null;
            }

            return new { key = referenceKey };
        }
    }
}