using RESTyard.Extensions.Pagination;

namespace RESTyard.AspNetCore.Extensions.Pagination;

/// <inheritdoc />
public abstract record QueryBase<TSortPropertyEnum, TQueryFilter, TPagination> : IQueryBase<TSortPropertyEnum, TQueryFilter>
    where TSortPropertyEnum : struct
    where TQueryFilter : IQueryFilter<TQueryFilter>
{
    /// <summary>
    /// Internal constructor to initialize the query with default values.
    /// </summary>
    protected QueryBase()
    {
        SortBy = SortParameter<TSortPropertyEnum>.CreateDefault();
        Filter = TQueryFilter.CreateDefault();
        Pagination = RESTyard.Extensions.Pagination.Pagination.CreateDefault();
    }

    /// <summary>
    /// The copy constructor.
    /// </summary>
    /// <param name="other"></param>
    protected QueryBase(QueryBase<TSortPropertyEnum, TQueryFilter, TPagination> other)
    {
        Pagination = new RESTyard.Extensions.Pagination.Pagination(other.Pagination.PageSize, other.Pagination.PageOffset);
        SortBy = new SortParameter<TSortPropertyEnum>(other.SortBy.PropertyName, other.SortBy.SortType);
        Filter = other.Filter.DeepCopy();
    }

    /// <inheritdoc />
    public RESTyard.Extensions.Pagination.Pagination Pagination { get; set; }

    /// <inheritdoc />
    public SortParameter<TSortPropertyEnum> SortBy { get; set; }

    /// <inheritdoc />
    public TQueryFilter Filter { get; set; }

    /// <inheritdoc />
    public abstract IQueryBase<TSortPropertyEnum, TQueryFilter> DeepCopy();
}