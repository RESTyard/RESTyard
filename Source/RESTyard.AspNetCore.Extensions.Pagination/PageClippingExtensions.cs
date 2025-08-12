namespace RESTyard.AspNetCore.Extensions.Pagination;

public static class PageClippingExtensions
{
    public static IEnumerable<T> ClipToPagination<T>(this IEnumerable<T> source, RESTyard.Extensions.Pagination.Pagination pagination) =>
        pagination.IsDisabled
            ? source
            : source.Skip(pagination.PageSize * pagination.PageOffset).Take(pagination.PageSize);
    
    public static IQueryable<T> ClipToPagination<T>(this IQueryable<T> source, RESTyard.Extensions.Pagination.Pagination pagination) =>
        pagination.IsDisabled
            ? source
            : source.Skip(pagination.PageSize * pagination.PageOffset).Take(pagination.PageSize);
}