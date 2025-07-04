namespace RESTyard.Extensions.Pagination;

/// <summary>
/// Defines a contract for pagination functionality in queries.
/// </summary>
/// <typeparam name="TSortPropertyEnum"></typeparam>
/// <typeparam name="TQueryFilter"></typeparam>
public interface IReadOnlyQueryBase<TSortPropertyEnum, out TQueryFilter> where TSortPropertyEnum : struct
{
    /// <summary>
    /// Pagination parameters
    /// </summary>
    Pagination Pagination { get; }

    /// <summary>
    /// The property to sort by and sort order
    /// </summary>
    SortParameter<TSortPropertyEnum> SortBy { get; }

    /// <summary>
    /// Filter object for this query.
    /// </summary>
    TQueryFilter Filter { get; }
}