using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RESTyard.AspNetCore.Extensions;

internal static class IEnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
        => source.Where(x => x is not null)!;
}