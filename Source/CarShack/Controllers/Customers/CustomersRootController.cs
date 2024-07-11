using System.Collections.Generic;
using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Util.Repository;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace CarShack.Controllers.Customers
{
    [Route("Customers/")]
    public class CustomersRootController : Controller
    {
        private readonly HypermediaCustomersRootHto customersRoot;
        private readonly ICustomerRepository customerRepository;

        public CustomersRootController(HypermediaCustomersRootHto customersRoot, ICustomerRepository customerRepository)
        {
            this.customersRoot = customersRoot;
            this.customerRepository = customerRepository;
        }

#region HypermediaObjects
        // Route to the HypermediaCustomersRoot. References to HypermediaCustomersRoot type will be resolved to this route.
        [HttpGetHypermediaObject("", typeof(HypermediaCustomersRootHto))]
        public ActionResult GetRootDocument()
        {
            return Ok(customersRoot);
        }

        // Building Queries using the CreateQuery will link to this route.
        [HttpGetHypermediaObject("Query", typeof(HypermediaCustomerQueryResultHto))]
        public async Task<ActionResult> Query([FromQuery] CustomerQuery query)
        {
            if (query == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            var queryResult = await customerRepository.QueryAsync(query).ConfigureAwait(false);
            var resultReferences = new List<HypermediaCustomerHto>();
            foreach (var customer in queryResult.Entities)
            {
                resultReferences.Add(customer.ToHto());
            }

            var queries = NavigationQuerysBuilder.Create(query, queryResult);
            var result = new HypermediaCustomerQueryResultHto(
                queryResult.TotalCountOfEnties,
                resultReferences.Count,
                resultReferences,
                queries.next.IsSome(),
                queries.next.GetValueOrDefault(),
                queries.previous.IsSome(),
                queries.previous.GetValueOrDefault(),
                queries.last.IsSome(),
                queries.last.GetValueOrDefault(),
                queries.all.IsSome(),
                queries.all.GetValueOrDefault(),
                query);
            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            result.AddNavigationQueries(navigationQuerys);
           
            return Ok(result);
        }
#endregion

#region Actions
        // Provides a link to the result Query.
        [HttpPostHypermediaAction("Queries", typeof(HypermediaCustomersRootHto.CreateQueryOp))]
        public ActionResult NewQueryAction(CustomerQuery query)
        {
            if (query == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            if (!customersRoot.CreateQuery.CanExecute())
            {
                return this.CanNotExecute();
            }

            // Will create a Location header with a URI to the result.
            return this.CreatedQuery(typeof(HypermediaCustomerQueryResultHto), query);
        }

        [HttpPostHypermediaAction("CreateCustomer", typeof(HypermediaCustomersRootHto.CreateCustomerOp))]
        public async Task<ActionResult> NewCustomerAction(CreateCustomerParameters createCustomerParameters)
        {
            if (createCustomerParameters == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            var createdCustomer = await CreateCustomer(createCustomerParameters);

            // Will create a Location header with a URI to the result.
            return this.Created(createdCustomer);
        }
        
        private async Task<HypermediaCustomerHto> CreateCustomer(CreateCustomerParameters createCustomerParameters)
        {
          
                var customer = CustomerService.CreateRandomCustomer();
                customer.Name = createCustomerParameters.Name;
                await customerRepository.AddEntityAsync(customer).ConfigureAwait(false);
                return customer.ToHto();
        }
#endregion
    }
}
