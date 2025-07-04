using System.Collections.Generic;
using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Extensions.Pagination;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace CarShack.Controllers.Customers
{
    [Route("Customers/")]
    [ApiController]
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
        [HttpGet(""), HypermediaObjectEndpoint<HypermediaCustomersRootHto>]
        public ActionResult GetRootDocument()
        {
            return Ok(customersRoot);
        }

        // Building Queries using the CreateQuery will link to this route.
        [HttpGet("Query"), HypermediaObjectEndpoint<HypermediaCustomerQueryResultHto>]
        public async Task<ActionResult> Query([FromQuery] CustomerQuery? query)
        {
            if (query == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            var queryResult = (await customerRepository.QueryAsync(query).ConfigureAwait(false)).GetValueOrThrow();
            var resultReferences = new List<HypermediaCustomerHto>();
            foreach (var customer in queryResult.Entities)
            {
                resultReferences.Add(customer.ToHto());
            }

            var queries = NavigationQueryBuilder.Create(query, queryResult);
            var result = new HypermediaCustomerQueryResultHto(
                queryResult.TotalCountOfEntities,
                resultReferences.Count,
                resultReferences,
                queries.next.Map(IHypermediaQuery (some) => some),
                queries.previous.Map(IHypermediaQuery (some) => some),
                queries.last.Map(IHypermediaQuery (some) => some),
                queries.all.Map(IHypermediaQuery (some) => some),
                query);
           
            return Ok(result);
        }
#endregion

#region Actions
        // Provides a link to the result Query.
        [HttpPost("Queries"), HypermediaActionEndpoint<HypermediaCustomersRootHto>(nameof(HypermediaCustomersRootHto.CreateQuery))]
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
            return this.Created(Link.ByQuery<HypermediaCustomerQueryResultHto>(query));
        }

        [HttpPost("CreateCustomer"), HypermediaActionEndpoint<HypermediaCustomersRootHto>(nameof(HypermediaCustomersRootHto.CreateCustomer))]
        public async Task<ActionResult> NewCustomerAction(CreateCustomerParameters createCustomerParameters)
        {
            if (createCustomerParameters == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            var createdCustomer = await CreateCustomer(createCustomerParameters);

            // Will create a Location header with a URI to the result.
            return this.Created(Link.To(createdCustomer));
        }
        
        private async Task<HypermediaCustomerHto> CreateCustomer(CreateCustomerParameters createCustomerParameters)
        {
            var customer = CustomerService.CreateRandomCustomer(isFavorite: false);
            customer.Name = createCustomerParameters.Name;
            await customerRepository.AddEntityAsync(customer).ConfigureAwait(false);
            return customer.ToHto();
        }
#endregion
    }
}
