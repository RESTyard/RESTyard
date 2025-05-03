using System;
using System.Collections.Generic;
using System.Linq;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Query
{
    public class NavigationQueries
    {
        public void AddQuery(string relation, IHypermediaQuery query)
        {
            Queries.Add(relation, query);
        }

        public Dictionary<string, IHypermediaQuery> Queries { get; } = new Dictionary<string, IHypermediaQuery>();

        public IEnumerable<Link> ToLinks<THto>() where THto : HypermediaQueryResult
            => Queries.Select(kvp => new Link(kvp.Key, new HypermediaObjectQueryReference(typeof(THto), kvp.Value)));
    }
}