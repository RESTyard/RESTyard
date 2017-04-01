using System.Collections.Generic;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Hypermedia
{
    /// <summary>
    /// A Entity which is related to a HypermediaObject.
    /// </summary>
    public class RelatedEntity
    {
        /// <summary>
        /// Describes the relations that the related entity has to the embedding entity.
        /// This allows to give the related entities extra context similar to the relations used by Links.
        /// </summary>
        public List<string> Relations { get; set; }

        /// <summary>
        /// A reference to the related entity.
        /// </summary>
        public HypermediaObjectReferenceBase Reference { get; set; }

        /// <summary>
        /// Cretaes a RelatedEntity.
        /// </summary>
        /// <param name="relations">List of relations as string, should at least contain one relation.</param>
        /// <param name="reference">Reference to the related Entity.</param>
        public RelatedEntity(List<string> relations, HypermediaObjectReferenceBase reference)
        {
            Relations = new List<string>(relations);
            Reference = reference;
        }

        /// <summary>
        /// Cretaes a RelatedEntity.
        /// </summary>
        /// <param name="relation">Relation string.</param>
        /// <param name="reference">Reference to the related Entity.</param>
        public RelatedEntity(string relation, HypermediaObjectReferenceBase reference)
        {
            Relations = new List<string>{relation};
            Reference = reference;
        }
    }
}