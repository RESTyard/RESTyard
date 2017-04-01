using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Extensions
{
    /// <summary>
    /// Convenience functions to add related Entities
    /// </summary>
    public static class EntitiesListExtensions
    {
        /// <summary>
        /// Adds a range of Entities using the same realations list.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relations">Relations for the Entities.</param>
        /// <param name="entities">The added Entities.</param>
        public static void AddRange(this List<RelatedEntity> entitiesList, List<string> relations, IEnumerable<HypermediaObjectReferenceBase> entities)
        {
            entitiesList.AddRange(entities.Select(entity => new RelatedEntity(relations, entity)));
        }

        /// <summary>
        /// Adds a range of Entities using the same realations list.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relations">Relations for the Entities.</param>
        /// <param name="entities">The added Entities.</param>
        public static void AddRange(this List<RelatedEntity> entitiesList, List<string> relations, IEnumerable<HypermediaObject> entities)
        {
            entitiesList.AddRange(entities.Select(entity => new RelatedEntity(relations, new HypermediaObjectReference(entity))));
        }

        /// <summary>
        /// Adds a range of Entities using the same realation.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relation">Relation for the Entities.</param>
        /// <param name="entities">The added Entities.</param>
        public static void AddRange(this List<RelatedEntity> entitiesList, string relation, IEnumerable<HypermediaObjectReferenceBase> entities)
        {
            entitiesList.AddRange(entities.Select(entity => new RelatedEntity(relation, entity)));
        }

        /// <summary>
        /// Adds a Entity using the same realation.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relation">Relation for the Entities.</param>
        /// <param name="entities">The added Entities.</param>
        public static void AddRange(this List<RelatedEntity> entitiesList, string relation, IEnumerable<HypermediaObject> entities)
        {
            entitiesList.AddRange(entities.Select(entity => new RelatedEntity(relation, new HypermediaObjectReference(entity))));
        }

        /// <summary>
        /// Adds a Entity using a single realation.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relation">Relation for the Entity.</param>
        /// <param name="entity">The added Entity.</param>
        public static void Add(this List<RelatedEntity> entitiesList, string relation, HypermediaObjectReferenceBase entity)
        {
            entitiesList.Add(new RelatedEntity(relation, entity));
        }

        /// <summary>
        /// Adds a range of Entity using a single realation.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relation">Relation for the Entity.</param>
        /// <param name="entity">The added Entity.</param>
        public static void Add(this List<RelatedEntity> entitiesList, string relation, HypermediaObject entity)
        {
            entitiesList.Add(new RelatedEntity(relation, new HypermediaObjectReference(entity)));
        }

        /// <summary>
        /// Adds Entity using a list of realations.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relations">Relations for the Entity.</param>
        /// <param name="entity">The added Entity.</param>
        public static void Add(this List<RelatedEntity> entitiesList, List<string> relations, HypermediaObjectReferenceBase entity)
        {
            entitiesList.Add(new RelatedEntity(relations, entity));
        }

        /// <summary>
        /// Adds Entity using a list of realations.
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relations">Relations for the Entity.</param>
        /// <param name="entity">The added Entity.</param>
        public static void Add(this List<RelatedEntity> entitiesList, List<string> relations, HypermediaObject entity)
        {
            entitiesList.Add(new RelatedEntity(relations, new HypermediaObjectReference(entity)));
        }

        /// <summary>
        /// Returns RelatedEntity where the class is matching by calling GetInstance() for each contained HypermediaObjectReferenceBase.
        /// If no Instance can be resolved no object is yielded.
        /// </summary>
        /// <typeparam name="T">Desired HypermediaObject Type</typeparam>
        /// <param name="entitiesList">Entities List.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetInstanceByClass<T>(this List<RelatedEntity> entitiesList) where T : HypermediaObject
        {
            return entitiesList
                .FilterByClass<T>()
                .Select(ee => ee.Reference.GetInstance() as T);
        }

        /// <summary>
        /// Filters the list by related HypermediaObject Type.
        /// </summary>
        /// <typeparam name="T">Desired HypermediaObject Type</typeparam>
        /// <param name="entitiesList">Entities List.</param>
        /// <returns></returns>
        public static IEnumerable<RelatedEntity> FilterByClass<T>(this List<RelatedEntity> entitiesList) where T : HypermediaObject
        {
            var lookupType = typeof(T);
            return entitiesList.Where(ee => lookupType.IsAssignableFrom(ee.Reference.GetHypermediaType()));
        }

        /// <summary>
        /// Filters the list by a given relation. 
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relation">Relation which must be found in the result.</param>
        /// <returns></returns>
        public static IEnumerable<RelatedEntity> FilterByRelation(this List<RelatedEntity> entitiesList, string relation)
        {
            return entitiesList.Where(ee => ee.Relations.Contains(relation));
        }

        /// <summary>
        /// Filters the list by a given relations. 
        /// </summary>
        /// <param name="entitiesList">Entities List.</param>
        /// <param name="relations">Relations which must be found in the result.</param>
        /// <returns></returns>
        public static IEnumerable<RelatedEntity> FilterByRelations(this List<RelatedEntity> entitiesList, ICollection<string> relations)
        {
            foreach (var entity in entitiesList)
            {
                var allRelationsFound = relations.All(relation => entity.Relations.Contains(relation));
                if (allRelationsFound)
                {
                    yield return entity;
                }
            }
        }
    }
}