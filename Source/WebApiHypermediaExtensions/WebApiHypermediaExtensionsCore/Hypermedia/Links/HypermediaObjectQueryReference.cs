using System;
using WebApiHypermediaExtensionsCore.Query;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Links
{
    /// <summary>
    /// A reference to an <see cref="IHypermediaQuery"/> where the Type and Query is known.
    /// Allows to referenc an <see cref="HypermediaQueryResult"/> without creating it for reference purpose only.
    /// </summary>
    public class HypermediaObjectQueryReference : HypermediaObjectReferenceBase
    {
        public IHypermediaQuery Query { get; }

        public HypermediaObjectQueryReference(Type hypermediaObjectType, IHypermediaQuery query) : base(hypermediaObjectType)
        {
            Query = query;
        }
        
        public IHypermediaQuery GetQuery()
        {
            return Query;
        }
    }
}