using System.Collections.Generic;
using System.Linq;
using RESTyard.AspNetCore.Util.Repository;

namespace CarShack.Util.Repository
{
    public abstract class RepositoryBase<T>
    {

        public IEnumerable<T> ClipToPagination(IEnumerable<T> enumerable, Pagination pagination)
        {
            if (pagination.PageSize == 0)
            {
                return enumerable;
            }

            return enumerable.Skip(pagination.PageOffset).Take(pagination.PageSize);
        }
    }
}