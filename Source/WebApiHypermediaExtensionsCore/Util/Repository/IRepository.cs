using System.Threading.Tasks;

namespace WebApiHypermediaExtensionsCore.Util.Repository
{
    public interface IRepository<TEntity, TKey, TQuery>
    {
        Task<TEntity> GetEnitityByKeyAsync(TKey key);

        Task<QueryResult<TEntity>> QueryAsync(TQuery query);

        Task AddEntityAsync(TEntity entity);
    }
}
