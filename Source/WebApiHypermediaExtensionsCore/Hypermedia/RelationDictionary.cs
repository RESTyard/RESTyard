using System.Collections.Generic;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using Hypermedia.Util;

namespace WebApiHypermediaExtensionsCore.Hypermedia
{
    /// <summary>
    /// Holds Links to related HypermediaObjects.
    /// Relations are treated as equal if all entries are provided by both lists and no additional entry is pressent.
    /// </summary>
    public class RelationDictionary : Dictionary<List<string>, RelatedEntity>
    {
        public RelationDictionary() : base(new StringCollectionComparer())
        {
        }

        /// <summary>
        /// Add a related Entity to the Links Dictionary.
        /// If a related entity with the same relation is pressent it is replaced.
        /// </summary>
        /// <param name="relatedEntity">To be added.</param>
        public void Add(RelatedEntity relatedEntity)
        {
            this[relatedEntity.Relations] = relatedEntity;
        }

        /// <summary>
        /// Convenience function: Add a HypermediaObjectReferenceBase to the Links Dictionary.
        /// If a related entity with the same relation is pressent it is replaced.
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
        /// If a related entity with the same relations is pressent it is replaced.
        /// </summary>
        /// <param name="relations">The relations to use</param>
        /// <param name="reference">To be added.</param>
        public void Add(List<string> relations, HypermediaObjectReferenceBase reference)
        {
            var relatedEntity = new RelatedEntity(relations, reference);
            Add(relatedEntity);
        }

        /// <summary>
        /// Convenience function: Add a HypermediaObject to the Links Dictionary.
        /// If a related entity with the same relation is pressent it is replaced.
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
        /// If a related entity with the same relations is pressent it is replaced.
        /// </summary>
        /// <param name="relations">The relations to use</param>
        /// <param name="reference">To be added.</param>
        public void Add(List<string> relations, HypermediaObject reference)
        {
            var relatedEntity = new RelatedEntity(relations, new HypermediaObjectReference(reference));
            Add(relatedEntity);
        }
    }
}
