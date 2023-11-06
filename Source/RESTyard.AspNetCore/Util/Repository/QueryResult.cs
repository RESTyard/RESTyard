using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTyard.AspNetCore.Util.Repository
{
    public class QueryResult<T>
    {
        public IEnumerable<T> Entities { get; set; } = Enumerable.Empty<T>();

        /// All that are available in the repository
        public int TotalCountOfEnties { get; set; }
    }
}