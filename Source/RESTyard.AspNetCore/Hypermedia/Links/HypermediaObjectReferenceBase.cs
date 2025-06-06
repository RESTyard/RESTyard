using System;
using System.Reflection;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Hypermedia.Links
{
    public abstract class HypermediaObjectReferenceBase
    {
        protected readonly Type HypermediaObjectType;

        protected HypermediaObjectReferenceBase(Type hypermediaObjectType)
        {
            if (!AttributedRouteHelper.Has<HypermediaObjectAttribute>(hypermediaObjectType))
            {
                throw new HypermediaException($"Type does not have {typeof(HypermediaObjectAttribute)}.");
            }
            
            this.HypermediaObjectType = hypermediaObjectType;
        }

        /// <summary>
        ///  Indicates if this reference can be resolved to a instance.
        /// </summary>
        public abstract bool CanResolve();

        /// <summary>
        /// Derived classes may choose to return the actual referenced <see cref="IHypermediaObject"/>
        /// </summary>
        /// <returns></returns>
        public abstract IHypermediaObject? GetInstance();

        /// <summary>
        ///  Indicates if this reference is backed by a instance.
        /// </summary>
        public abstract bool IsResolved();

        /// <summary>
        ///  Creates a object which can be used as key for the referenced. 
        /// </summary>
        /// <param name="keyProducer"></param>
        /// <returns>The key object to this <see cref="IHypermediaObject"/> or null if it has no key.</returns>
        public abstract object? GetKey(IKeyProducer keyProducer);

        /// <summary>
        /// Return a related query object. Null if none is related.
        /// </summary>
        /// <returns></returns>
        public abstract IHypermediaQuery? GetQuery();

        /// <summary>
        /// Get the referenced <see cref="IHypermediaObject"/> type
        /// </summary>
        /// <returns>Referenced Type</returns>
        public Type GetHypermediaType()
        {
            return this.HypermediaObjectType;
        }
    }
}