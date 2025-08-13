namespace RESTyard.AspNetCore.Extensions.Pagination;

/// <summary>
/// Represents the result of a query operation that includes a collection of entities and their total count that match the query criteria, regardless of pagination.
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public interface IQueryResult<out TEntity>
{
    /// <summary>
    /// Represents the collection of entities returned by a query.
    /// </summary>
    IEnumerable<TEntity> Entities { get; }

    /// <summary>
    /// Represents the total count of entities that match the query criteria, regardless of pagination.
    /// </summary>
    int TotalCountOfEntities { get; }
}