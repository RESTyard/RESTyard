using System;
using System.Collections.Generic;

namespace RESTyard.AspNetCore.Util.Repository
{
    public class QueryResult<T>
    {
        public IEnumerable<T> Entities { get; set; }

        /// All that are available in the repository
        public int TotalCountOfEnties { get; set; }
    }
}