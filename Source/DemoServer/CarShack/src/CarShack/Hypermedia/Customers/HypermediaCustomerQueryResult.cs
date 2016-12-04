using System.Collections.Generic;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Query;
using CarShack.Domain.Customer;

namespace CarShack.Hypermedia.Customers
{
    
    [HypermediaObject(Title = "Query result on Customers", Classes = new[] { "Customers", "QueryResult" })]
    public class HypermediaCustomerQueryResult : HypermediaQueryResult
    {
        public int TotalEnties { get; set; }

        public int CurrentEntitiesCount { get; set; }

        // The resulting HypermediaObject when it is a Query. Requires the query object so the self link can be build including the query string.
        // NavigationQuerys are additional Links like e.g. pagination "Next" or "First"
        public HypermediaCustomerQueryResult(IList<HypermediaObjectReferenceBase> entities, int totalEnties, CustomerQuery query, NavigationQueries navigationQuerys = null)
            : base(entities, query, navigationQuerys)
        {
            TotalEnties = totalEnties;
            CurrentEntitiesCount = entities.Count;
        }
    }
}
