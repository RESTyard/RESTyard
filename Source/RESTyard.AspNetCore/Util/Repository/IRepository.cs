using System;
using System.Threading.Tasks;

namespace RESTyard.AspNetCore.Util.Repository
{
    public interface IRepository<TEntity, in TKey, in TQuery>
    {
        Task<TEntity> GetEntityByKeyAsync(TKey key);

        Task<QueryResult<TEntity>> QueryAsync(TQuery query);

        Task AddEntityAsync(TEntity entity);
    }
}
