using System;

namespace RESTyard.WebApi.Extensions.Util.Repository
{
    public interface IQueryFilter
    {
        IQueryFilter Clone();
    }
}