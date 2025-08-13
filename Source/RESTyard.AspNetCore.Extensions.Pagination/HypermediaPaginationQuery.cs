using RESTyard.Extensions.Pagination;

namespace RESTyard.AspNetCore.Extensions.Pagination;

/// <inheritdoc cref="IHypermediaPaginationQuery{TSortPropertyEnum,TQueryFilter}" />
public abstract record HypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> : IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter>
    where TSortPropertyEnum : struct
    where TQueryFilter : IQueryFilter<TQueryFilter>
{
    /// <summary>
    /// Internal constructor to initialize the query with default values.
    /// </summary>
    protected HypermediaPaginationQuery()
    {
        Pagination = RESTyard.Extensions.Pagination.Pagination.DisabledPagination;
        SortBy = [];
        Filter = TQueryFilter.CreateDefault();
    }

    /// <inheritdoc cref="IHypermediaPaginationQuery{TSortPropertyEnum,TQueryFilter}" />
    public RESTyard.Extensions.Pagination.Pagination Pagination { get; set; }

    /// <inheritdoc cref="IHypermediaPaginationQuery{TSortPropertyEnum,TQueryFilter}" />
    public List<Sorting<TSortPropertyEnum>> SortBy { get; set; }

    /// <inheritdoc cref="IHypermediaPaginationQuery{TSortPropertyEnum,TQueryFilter}" />
    public TQueryFilter Filter { get; set; }

    /// <inheritdoc />
    public abstract IHypermediaPaginationQuery<TSortPropertyEnum, TQueryFilter> DeepCopy();
}