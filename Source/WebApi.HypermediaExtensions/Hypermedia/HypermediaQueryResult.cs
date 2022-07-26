using System;
using RESTyard.WebApi.Extensions.Hypermedia.Attributes;
using RESTyard.WebApi.Extensions.Hypermedia.Links;
using RESTyard.WebApi.Extensions.Query;

namespace RESTyard.WebApi.Extensions.Hypermedia
{
    /// <summary>
    /// Base class for query results.
    /// </summary>
    public abstract class HypermediaQueryResult : HypermediaObject 
    {
        [FormatterIgnoreHypermediaProperty]
        public IHypermediaQuery Query { get; }

        /// <summary>
        /// Base class for query results.
        /// </summary>
        /// <param name="query">The query used to retrieve this result.</param>
        protected HypermediaQueryResult(IHypermediaQuery query) : base (query)
        {
            Query = query;
        }

        /// <summary>
        /// Adds all Queries from a NavigationQueries as Link using this hypermediaObject type target.
        /// Existing if a Query is added for which a Link with the same relation exists it is replaced.
        /// </summary>
        /// <param name="navigationQueries">The Queries to add</param>
        public void AddNavigationQueries(NavigationQueries navigationQueries)
        {
            foreach (var navigationQuery in navigationQueries.Queries)
            {
                Links.Add(navigationQuery.Key, new HypermediaObjectQueryReference(GetType(), navigationQuery.Value));
            }
        }
    }
}