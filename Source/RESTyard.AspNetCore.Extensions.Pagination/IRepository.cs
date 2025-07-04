using FunicularSwitch;

namespace RESTyard.AspNetCore.Extensions.Pagination
{
    /// <summary>
    /// Represents a repository interface for managing entities with pagination and query capabilities.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity managed by the repository.</typeparam>
    /// <typeparam name="TKey">The type of the unique identifier for the entity.</typeparam>
    /// <typeparam name="TQuery">The type representing query parameters for filtering entities.</typeparam>
    public interface IRepository<TEntity, in TKey, in TQuery>
    {
        /// <summary>
        /// Gets an entity by its unique key asynchronously.
        /// </summary>
        Task<Result<Option<TEntity>>> GetEntityByKeyAsync(TKey key);

        /// <summary>
        /// Queries entities based on the provided query parameters asynchronously.
        /// </summary>
        Task<Result<IQueryResult<TEntity>>> QueryAsync(TQuery query);

        /// <summary>
        /// Adds a new entity to the repository asynchronously.
        /// </summary>
        Task<Result<TEntity>> AddEntityAsync(TEntity entity);
    }
}
