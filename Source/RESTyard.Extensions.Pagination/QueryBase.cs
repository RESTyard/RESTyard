namespace RESTyard.Extensions.Pagination;

/// <inheritdoc />
public abstract record QueryBase<TCustomerSortProperties, TCustomerFilter>(
    Pagination Pagination,
    SortParameter<TCustomerSortProperties> SortBy,
    TCustomerFilter Filter) : IReadOnlyQueryBase<TCustomerSortProperties, TCustomerFilter>
    where TCustomerSortProperties : struct
    where TCustomerFilter : IDeepCopyable<TCustomerFilter>;