namespace RESTyard.Extensions.Pagination;

/// <summary>
/// Defines a contract for pagination functionality in queries.
/// </summary>
public record Pagination(int PageSize, int PageOffset)
{
    /// <summary>
    /// Indicates whether pagination is enabled for the current query.
    /// </summary>
    /// <returns></returns>
    public bool HasPagination()
    {
        return PageSize > 0;
    }

    /// <summary>
    /// Disables pagination for the current query,
    /// meaning that all items will be returned without any pagination applied.
    /// </summary>
    public static Pagination DisablePagination() =>
        new(PageSize: 0, PageOffset: 0);

    /// <summary>
    /// Creates a new instance of the default pagination implementation.
    /// </summary>
    public static Pagination CreateDefault() => 
        DisablePagination();
}