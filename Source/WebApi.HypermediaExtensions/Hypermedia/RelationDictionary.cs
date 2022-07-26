using System;
using System.Collections.Generic;
using RESTyard.WebApi.Extensions.Hypermedia.Links;
using RESTyard.WebApi.Extensions.Util;

namespace RESTyard.WebApi.Extensions.Hypermedia
{
    /// <summary>
    /// Holds Links to related HypermediaObjects.
    /// Relations are treated as equal if all entries are provided by both lists and no additional entry is present.
    /// </summary>
    public class RelationDictionary : Dictionary<IReadOnlyCollection<string>, RelatedEntity>
    {
        public RelationDictionary() : base(new StringReadOnlyCollectionComparer())
        {
        }

        /// <summary>
        /// Add a related Entity to the Links Dictionary.
        /// If a related entity with the same relation is present it is replaced.
        /// </summary>
        /// <param name="relatedEntity">To be added.</param>
        public void Add(RelatedEntity relatedEntity)
        {
            this[relatedEntity.Relations] = relatedEntity;
        }

        /// <summary>
        /// Convenience function: Add a HypermediaObjectReferenceBase to the Links Dictionary.
        /// If a related entity with the same relation is present it is replaced.
        /// </summary>
        /// <param name="relation">The relation to use</param>
        /// <param name="reference">To be added.</param>
        public void Add(string relation, HypermediaObjectReferenceBase reference)
        {
            var relatedEntity = new RelatedEntity(relation, reference);
            Add(relatedEntity);
        }

        /// <summary>
        /// Convenience function: Add a HypermediaObjectReferenceBase to the Links Dictionary.
        /// If a related entity with the same relations is present it is replaced.
        /// </summary>
        /// <param name="relations">The relations to use</param>
        /// <param name="reference">To be added.</param>
        public void Add(IReadOnlyCollection<string> relations, HypermediaObjectReferenceBase reference)
        {
            var relatedEntity = new RelatedEntity(relations, reference);
            Add(relatedEntity);
        }

        /// <summary>
        /// Convenience function: Add a HypermediaObject to the Links Dictionary.
        /// If a related entity with the same relation is present it is replaced.
        /// </summary>
        /// <param name="relation">The relations to use</param>
        /// <param name="reference">To be added.</param>
        public void Add(string relation, HypermediaObject reference)
        {
            var relatedEntity = new RelatedEntity(relation, new HypermediaObjectReference(reference));
            Add(relatedEntity);
        }

        /// <summary>
        /// Convenience function: Add a HypermediaObject to the Links Dictionary.
        /// If a related entity with the same relations is present it is replaced.
        /// </summary>
        /// <param name="relations">The relations to use</param>
        /// <param name="reference">To be added.</param>
        public void Add(IReadOnlyCollection<string> relations, HypermediaObject reference)
        {
            var relatedEntity = new RelatedEntity(relations, new HypermediaObjectReference(reference));
            Add(relatedEntity);
        }
    }
}
