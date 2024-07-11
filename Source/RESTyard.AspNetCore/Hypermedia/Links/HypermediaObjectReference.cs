using System;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Hypermedia.Links
{
    /// <summary>
    /// Reference to a HypermediaObject. Use this if you already have a HypermediaObject.
    /// </summary>
    public class HypermediaObjectReference : HypermediaObjectReferenceBase
    {
        private readonly HypermediaObject reference;

        public HypermediaObjectReference(HypermediaObject hypermediaObject) : base(hypermediaObject.GetType())
        {
            if (hypermediaObject == null)
            {
                throw new HypermediaException("HypermediaObject is null.");
            }

            this.reference = hypermediaObject;
        }

        /// <summary>
        /// Resolves the referenced HypermediaObject
        /// </summary>
        /// <returns>The HypermediaObject.</returns>
        public override HypermediaObject? GetInstance()
        {
            return this.reference;
        }

        public override bool CanResolve()
        {
            return true;
        }

        public override bool IsResolved()
        {
            return true;
        }

        public override object? GetKey(IKeyProducer keyProducer)
        {
            return keyProducer.CreateFromHypermediaObject(this.reference);
        }

        public override IHypermediaQuery? GetQuery()
        {
            return null;
        }

        public static implicit operator HypermediaObjectReference(HypermediaObject hypermediaObject)
        {
            return new HypermediaObjectReference(hypermediaObject);
        }
    }
}