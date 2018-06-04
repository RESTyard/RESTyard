using System.Threading.Tasks;

namespace WebApi.HypermediaExtensions.Util.Repository
{
    public interface IRepository<TEntity, in TKey, in TQuery>
    {
        Task<TEntity> GetEnitityByKeyAsync(TKey key);

        Task<QueryResult<TEntity>> QueryAsync(TQuery query);

        Task AddEntityAsync(TEntity entity);
    }
}
