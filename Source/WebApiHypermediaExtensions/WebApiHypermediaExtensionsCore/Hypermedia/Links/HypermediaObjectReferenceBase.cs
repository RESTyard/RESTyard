using System;
using System.Reflection;
using WebApiHypermediaExtensionsCore.Exceptions;

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
        /// Derived classes may choose to return the actual referenced <see cref="HypermediaObject"/>
        /// </summary>
        /// <returns></returns>
        public virtual HypermediaObject Resolve()
        {
            // if required the reference must have access to a repository wich retrieves the domain object by key/query, pass repo in ctor, or factory delegate
            throw new NotImplementedException();
        }

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