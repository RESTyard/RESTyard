using System.Collections.Generic;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Hypermedia.Extensions;
using CarShack.Domain.Customer;
using Hypermedia.Util;

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
