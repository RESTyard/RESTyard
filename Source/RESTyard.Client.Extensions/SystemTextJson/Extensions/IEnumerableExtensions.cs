using System.Collections.Generic;
using System.Linq;

namespace RESTyard.Client.Extensions.SystemTextJson.Extensions;

internal static class IEnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
        => source.Where(x => x is not null)!;
}