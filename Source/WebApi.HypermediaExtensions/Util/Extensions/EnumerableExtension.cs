using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace WebApi.HypermediaExtensions.Util.Extensions
{
    static class EnumerableExtension
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static ImmutableArray<T> ToImmutableArraySafe<T>(this IEnumerable<T> items)
        {
            return items?.ToImmutableArray() ?? ImmutableArray<T>.Empty;
        }
    }
}
