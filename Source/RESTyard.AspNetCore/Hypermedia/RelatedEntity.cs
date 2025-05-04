using System;
using System.Collections.Generic;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Hypermedia
{
    /// <summary>
    /// A Entity which is related to a HypermediaObject.
    /// </summary>
    public abstract class RelatedEntity
    {
        /// <summary>
        /// A reference to the related entity.
        /// </summary>
        public HypermediaObjectReferenceBase Reference { get; set; }

        /// <summary>
        /// Creates a RelatedEntity.
        /// </summary>
        /// <param name="relations">List of relations as string, should at least contain one relation.</param>
        /// <param name="reference">Reference to the related Entity.</param>
        protected RelatedEntity(HypermediaObjectReferenceBase reference)
        {
            Reference = reference;
        }
    }

    public class Link : RelatedEntity
    {
        public Link(HypermediaObjectReferenceBase reference) : base(reference)
        {
        }
    }

    public class EmbeddedEntity : RelatedEntity
    {

        public EmbeddedEntity(HypermediaObjectReferenceBase reference) : base(reference)
        {
        }
    }
}