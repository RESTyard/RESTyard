using RESTyard.Extensions.Pagination;

namespace RESTyard.AspNetCore.Extensions.Pagination;

/// <inheritdoc cref="IHypermediaQueryBase{TSortPropertyEnum,TQueryFilter}" />
public abstract record HypermediaQueryBase<TSortPropertyEnum, TQueryFilter> : QueryBase<TSortPropertyEnum, TQueryFilter>,
    IHypermediaQueryBase<TSortPropertyEnum, TQueryFilter>
    where TSortPropertyEnum : struct
    where TQueryFilter : IQueryFilter<TQueryFilter>
{
    /// <summary>
    /// Internal constructor to initialize the query with default values.
    /// </summary>
    protected HypermediaQueryBase() : base(
        RESTyard.Extensions.Pagination.Pagination.CreateDefault(),
        SortParameter<TSortPropertyEnum>.CreateDefault(),
        TQueryFilter.CreateDefault())
    {
        Pagination = base.Pagination;
        SortBy = base.SortBy;
        Filter = base.Filter;
    }

    /// <inheritdoc cref="IHypermediaQueryBase{TSortPropertyEnum,TQueryFilter}" />
    public new RESTyard.Extensions.Pagination.Pagination Pagination { get; set; }

    /// <inheritdoc cref="IHypermediaQueryBase{TSortPropertyEnum,TQueryFilter}" />
    public new SortParameter<TSortPropertyEnum> SortBy { get; set; }

    /// <inheritdoc cref="IHypermediaQueryBase{TSortPropertyEnum,TQueryFilter}" />
    public new TQueryFilter Filter { get; set; }

    /// <inheritdoc />
    public abstract IHypermediaQueryBase<TSortPropertyEnum, TQueryFilter> DeepCopy();
}