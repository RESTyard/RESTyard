namespace RESTyard.AspNetCore.Extensions.Pagination
{
    /// <inheritdoc />
    public record QueryResult<T> : IQueryResult<T>
    {
        /// <inheritdoc />
        public IEnumerable<T> Entities { get; set; } = [];

        /// <inheritdoc />
        public int TotalCountOfEntities { get; set; }
    }
}