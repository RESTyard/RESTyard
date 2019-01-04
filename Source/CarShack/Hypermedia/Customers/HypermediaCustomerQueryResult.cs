using System.Collections.Generic;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Hypermedia.Extensions;
using CarShack.Domain.Customer;
using Bluehands.Hypermedia.Relations;

namespace CarShack.Hypermedia.Customers
{
    
    [HypermediaObject(Title = "Query result on Customers", Classes = new[] { "CustomersQueryResult" })]
    public class HypermediaCustomerQueryResult : HypermediaQueryResult
    {
        public int TotalEnties { get; set; }

        public int CurrentEntitiesCount { get; set; }

        // The resulting HypermediaObject when it is a Query. Requires the query object so the self link can be build including the query string.
        // NavigationQuerys are additional Links like e.g. pagination "Next" or "First"
        public HypermediaCustomerQueryResult(ICollection<HypermediaObjectReferenceBase> entities, int totalEnties, CustomerQuery query)
            : base(query)
        {
            TotalEnties = totalEnties;
            CurrentEntitiesCount = entities.Count;
            Entities.AddRange(DefaultHypermediaRelations.EmbeddedEntities.Item, entities);
        }
    }
}
