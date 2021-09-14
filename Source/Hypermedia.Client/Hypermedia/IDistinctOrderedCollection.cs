using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    /// <summary>
    /// Represents an immutable collection that contains elements in a certain order, and does not contain duplicate elements
    /// </summary>
    public interface IDistinctOrderedCollection<out TElement> : IEnumerable<TElement>
    {
        int Count { get; }
    }
}