using System.Collections.Generic;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Hypermedia
{
    /// <summary>
    /// A Entity which is embedded in a HypermediaObject.
    /// </summary>
    public class EmbeddedEntity
    {
        /// <summary>
        /// Describes the relations that the embedded entity has to the embedding entity.
        /// This allows to give the embedded entities extra context similar to the relations used by Links.
        /// </summary>
        public List<string> Relations { get; set; }

        /// <summary>
        /// A reference to the embedded entity.
        /// </summary>
        public HypermediaObjectReferenceBase Reference { get; set; }

        /// <summary>
        /// Cretaes a EmbeddedEntity.
        /// </summary>
        /// <param name="relations">List of relations as string, should at least contain one relation.</param>
        /// <param name="reference">Reference to the embedded Entity.</param>
        public EmbeddedEntity(List<string> relations, HypermediaObjectReferenceBase reference)
        {
            Relations = relations;
            Reference = reference;
        }
    }
}