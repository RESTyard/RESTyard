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

        protected HypermediaQueryResult(IEnumerable<HypermediaObjectReferenceBase> entities, IHypermediaQuery query, NavigationQueries navigationQuerys = null) : base (query)
        {
            Query = query;
            Entities.AddRange(entities);

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