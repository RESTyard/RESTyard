using System;
using System.Collections.Generic;

namespace RESTyard.Client.Hypermedia
{
    /// <summary>
    /// Represents an immutable collection that contains elements in a certain order, and does not contain duplicate elements
    /// </summary>
    public interface IDistinctOrderedCollection<out TElement> : IEnumerable<TElement>
    {
        int Count { get; }
    }
}