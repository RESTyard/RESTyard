using System;
using RESTyard.AspNetCore.Query;

namespace RESTyard.AspNetCore.Hypermedia.Links
{
    /// <summary>
    /// A reference to an <see cref="IHypermediaQuery"/> where the Type and Query is known.
    /// Allows to referenc an <see cref="HypermediaQueryResult"/> without creating it for reference purpose only.
    /// </summary>
    public class HypermediaObjectQueryReference : HypermediaObjectKeyReference
    {
        public IHypermediaQuery Query { get; }

        public HypermediaObjectQueryReference(Type hypermediaObjectType, IHypermediaQuery query, object key = null) : base(hypermediaObjectType, key)
        {
            Query = query;
        }
        
        public override IHypermediaQuery GetQuery()
        {
            return Query;
        }
    }
}