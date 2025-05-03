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
        /// Describes the relations that the related entity has to the embedding entity.
        /// This allows to give the related entities extra context similar to the relations used by Links.
        /// </summary>
        public IReadOnlyCollection<string> Relations { get; set; }

        /// <summary>
        /// A reference to the related entity.
        /// </summary>
        public HypermediaObjectReferenceBase Reference { get; set; }

        /// <summary>
        /// Creates a RelatedEntity.
        /// </summary>
        /// <param name="relations">List of relations as string, should at least contain one relation.</param>
        /// <param name="reference">Reference to the related Entity.</param>
        protected RelatedEntity(IReadOnlyCollection<string> relations, HypermediaObjectReferenceBase reference)
        {
            Relations = new List<string>(relations);
            Reference = reference;
        }

        /// <summary>
        /// Cretaes a RelatedEntity.
        /// </summary>
        /// <param name="relation">Relation string.</param>
        /// <param name="reference">Reference to the related Entity.</param>
        protected RelatedEntity(string relation, HypermediaObjectReferenceBase reference)
        {
            Relations = new List<string>{relation};
            Reference = reference;
        }
    }

    public class Link : RelatedEntity
    {
        public Link(IReadOnlyCollection<string> relations, HypermediaObjectReferenceBase reference) : base(relations, reference)
        {
        }

        public Link(string relation, HypermediaObjectReferenceBase reference) : base(relation, reference)
        {
        }
    }

    public class EmbeddedEntity : RelatedEntity
    {
        public EmbeddedEntity(IReadOnlyCollection<string> relations, HypermediaObjectReferenceBase reference) : base(relations, reference)
        {
        }

        public EmbeddedEntity(string relation, HypermediaObjectReferenceBase reference) : base(relation, reference)
        {
        }
    }
}