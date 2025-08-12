namespace RESTyard.Extensions.Pagination;

/// <summary>
/// Defines a contract for pagination functionality in queries.
/// </summary>
public readonly record struct Pagination(int PageSize, int PageOffset)
{
    
    /// <summary>
    /// Indicates whether pagination is disabled for the current query.
    /// </summary>
    public bool IsDisabled => this == DisabledPagination;

    /// <summary>
    /// Disables pagination for the current query,
    /// meaning that all items will be returned without any pagination applied.
    /// </summary>
    public static Pagination DisabledPagination => new(PageSize: 0, PageOffset: 0);

    /// <summary>
    /// Creates a new instance of the default pagination implementation.
    /// </summary>
    public static Pagination CreateDefault() => DisabledPagination;
}