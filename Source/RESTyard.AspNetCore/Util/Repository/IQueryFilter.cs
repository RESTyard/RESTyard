using System;

namespace RESTyard.AspNetCore.Util.Repository
{
    public interface IQueryFilter
    {
        IQueryFilter Clone();
    }
}