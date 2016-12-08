using WebApiHypermediaExtensionsCore.Hypermedia.Extensions;
using WebApiHypermediaExtensionsCore.Query;

namespace WebApiHypermediaExtensionsCore.Hypermedia
{
    using System.Collections.Generic;

    using Links;

    /// <summary>
    /// Base class for query results.
    /// </summary>
    public abstract class HypermediaQueryResult : HypermediaObject 
    {
        public IHypermediaQuery Query { get; }

        /// <summary>
        /// Base class for query results.
        /// </summary>
        /// <param name="entities">Entities which shall be embedded in the HypermediaQueryResult.</param>
        /// <param name="query">The query used to retrieve this result.</param>
        /// <param name="navigationQuerys">Optional container with additional Links.</param>
        protected HypermediaQueryResult(IEnumerable<HypermediaObjectReferenceBase> entities, IHypermediaQuery query, NavigationQueries navigationQuerys = null) : base (query)
        {
            Query = query;
            Entities.AddRange(DefaultHypermediaRelations.EmbeddedEntities.Item, entities);

            if (navigationQuerys != null)
            {
                foreach (var navigationQuery in navigationQuerys.Queries)
                {
                    Links[navigationQuery.Key] = new HypermediaObjectQueryReference(GetType(), navigationQuery.Value);
                }
            }
        }
    }
}