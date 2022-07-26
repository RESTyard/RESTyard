using System;
using System.Collections.Generic;

namespace RESTyard.WebApi.Extensions.Util.Extensions
{
    static class EnumerableExtension
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
