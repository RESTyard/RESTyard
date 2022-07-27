using System;
using System.Collections.Generic;

namespace RESTyard.AspNetCore.Query
{
    public class NavigationQueries
    {
        public void AddQuery(string relation, IHypermediaQuery query)
        {
            Queries.Add(relation, query);
        }

        public Dictionary<string, IHypermediaQuery> Queries { get; } = new Dictionary<string, IHypermediaQuery>();
    }
}