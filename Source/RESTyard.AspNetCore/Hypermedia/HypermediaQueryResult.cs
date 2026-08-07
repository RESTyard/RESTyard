using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Query;

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
        }
    }
}