using RESTyard.Extensions.Pagination;

namespace RESTyard.AspNetCore.Extensions.Pagination;

/// <summary>
/// Defines a contract for filtering query results in paginated collections.
/// Implementations of this interface provide criteria to filter data sets
/// before pagination is applied. The interface ensures that filters can be
/// safely copied when needed for stateful operations.
/// </summary>
public interface IQueryFilter<out TInstance> : IDeepCopyable<TInstance>
{
    /// <summary>
    /// Creates a new instance of the default pagination implementation.
    /// </summary>
    public static abstract TInstance CreateDefault();
}