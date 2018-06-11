using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.HypermediaExtensions.Util.Extensions
{
    static class EnumerableExtension
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
