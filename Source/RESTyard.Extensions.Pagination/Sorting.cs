namespace RESTyard.Extensions.Pagination;

/// <summary>
/// Represents a sort parameter for sorting entities in a query.
/// </summary>
/// <typeparam name="TSortIdentifier">The type that identifies sortable properties,
/// typically an enum defining available sort fields.
/// The type must have a valid default.</typeparam>
public record Sorting<TSortIdentifier>(TSortIdentifier Id, SortOrder Order = SortOrder.Default)
    where TSortIdentifier : struct
{
    /// <summary>
    /// Creates a new instance of the <see cref="Sorting{TSortIdentifier}"/> with the specified property name and sort type.
    /// </summary>
    /// <returns></returns>
    public static Sorting<TSortIdentifier> CreateDefault()
    {
        return new Sorting<TSortIdentifier>(default(TSortIdentifier));
    }
}