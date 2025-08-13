namespace RESTyard.Extensions.Pagination;

/// <inheritdoc />
public abstract record PaginationQuery<TSortIdentifier, TCustomerFilter>(
    Pagination Pagination,
    IReadOnlyCollection<Sorting<TSortIdentifier>> SortBy,
    TCustomerFilter Filter) : IPaginationQuery<TSortIdentifier, TCustomerFilter>
    where TSortIdentifier : struct
    where TCustomerFilter : IDeepCopyable<TCustomerFilter>;