namespace RESTyard.Extensions.Pagination
{
    /// <summary>
    /// Represents a sort parameter for sorting entities in a query.
    /// </summary>
    /// <typeparam name="TSortIdentifier">The type that identifies sortable properties, typically an enum defining available sort fields</typeparam>
    public record SortParameter<TSortIdentifier>(TSortIdentifier? PropertyName, SortTypes SortType)
        where TSortIdentifier : struct
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SortParameter{TSortIdentifier}"/> with the specified property name and sort type.
        /// </summary>
        /// <returns></returns>
        public static SortParameter<TSortIdentifier> CreateDefault()
        {
            return new SortParameter<TSortIdentifier>(default(TSortIdentifier), SortTypes.None);
        }
    }
}