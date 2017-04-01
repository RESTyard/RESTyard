using System;
using System.Reflection;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Links
{
    public abstract class HypermediaObjectReferenceBase
    {
        protected readonly Type HypermediaObjectType;

        protected HypermediaObjectReferenceBase(Type hypermediaObjectType)
        {
            if (!typeof(HypermediaObject).IsAssignableFrom(hypermediaObjectType))
            {
                throw new HypermediaException($"Type does not derive from {typeof(HypermediaObject)}.");
            }

            this.HypermediaObjectType = hypermediaObjectType;
        }

        /// <summary>
        ///  Indicates if this reference can be resolved to a instance.
        /// </summary>
        public abstract bool CanResolve();

        /// <summary>
        /// Derived classes may choose to return the actual referenced <see cref="HypermediaObject"/>
        /// </summary>
        /// <returns></returns>
        public abstract HypermediaObject GetInstance();

        /// <summary>
        ///  Indicates if this reference is backed by a instance.
        /// </summary>
        public abstract bool IsResolved();

        /// <summary>
        ///  Creates a object which can be used as key for the referenced. 
        /// </summary>
        /// <param name="keyProducer"></param>
        /// <returns>The key object to this <see cref="HypermediaObject"/> or null if it has no key.</returns>
        public abstract object GetKey(IKeyProducer keyProducer);

        /// <summary>
        /// Return a related query object. Null if none is related.
        /// </summary>
        /// <returns></returns>
        public abstract IHypermediaQuery GetQuery();

        /// <summary>
        /// Get the referenced <see cref="HypermediaObject"/> type
        /// </summary>
        /// <returns>Referenced Type</returns>
        public Type GetHypermediaType()
        {
            return this.HypermediaObjectType;
        }
    }
}