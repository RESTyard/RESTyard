using System.Collections.Generic;
using System.Linq;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Extensions
{
    /// <summary>
    /// Convenience functions to add embedded Entities
    /// </summary>
    public static class EntitiesListExtensions
    {
        /// <summary>
        /// Adds a range of Entities using the same realtions list.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relations">Relations for the Entities.</param>
        /// <param name="entities">The added Entities.</param>
        public static void AddRange(this List<EmbeddedEntity> entitiesList, List<string> relations, IEnumerable<HypermediaObjectReferenceBase> entities)
        {
            entitiesList.AddRange(entities.Select(entity => new EmbeddedEntity(relations, entity)));
        }

        /// <summary>
        /// Adds a range of Entities using the same realtion.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relation">Relation for the Entities.</param>
        /// <param name="entities">The added Entities.</param>
        public static void AddRange(this List<EmbeddedEntity> entitiesList, string relation, IEnumerable<HypermediaObjectReferenceBase> entities)
        {
            var relationsList = new List<string> { relation };
            entitiesList.AddRange(entities.Select(entity => new EmbeddedEntity(relationsList, entity)));
        }

        /// <summary>
        /// Adds a range of Entity using a single realtion.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relation">Relation for the Entity.</param>
        /// <param name="entity">The added Entity.</param>
        public static void Add(this List<EmbeddedEntity> entitiesList, string relation, HypermediaObjectReferenceBase entity)
        {
            var relationsList = new List<string> { relation };
            entitiesList.Add(new EmbeddedEntity(relationsList, entity));
        }
    }
}