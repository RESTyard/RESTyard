using System;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.Relations;

namespace RESTyard.AspNetCore.Hypermedia
{
    /// <summary>
    /// Base class for query results.
    /// </summary>
    public abstract class HypermediaQueryResult : IHypermediaQueryResult, IHypermediaObject
    {
        [FormatterIgnoreHypermediaProperty]
        public IHypermediaQuery Query { get; }

        /// <summary>
        /// Base class for query results.
        /// </summary>
        /// <param name="query">The query used to retrieve this result.</param>
        protected HypermediaQueryResult(IHypermediaQuery query)
        {
            Query = query;
            Self = new(new HypermediaObjectQueryReference(GetType(), query));
        }
        
        [Relations([DefaultHypermediaRelations.Self])]
        public Link<HypermediaQueryResult> Self { get; }
    }
}