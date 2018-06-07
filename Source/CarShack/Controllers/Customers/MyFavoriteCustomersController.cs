using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia.Customers;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;

namespace CarShack.Controllers.Customers
{
    [Route("MyFavoriteCustomers")]
    public class MyFavoriteCustomersController : Controller
    {
        private readonly ICustomerRepository customerRepository;

        public MyFavoriteCustomersController(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
        }

        #region TypeRoutes
        // Provide type information for Action parameters. Does not depend on a specific customer.
        [HttpGetHypermediaActionParameterInfo("FavoriteCustomer", typeof(FavoriteCustomer))]
        public async Task<ActionResult> FavoriteCustomerType()
        {
            var schema = await JsonSchemaFactory.Generate(typeof(FavoriteCustomer)).ConfigureAwait(false);
            return Ok(schema);
        }
        #endregion
    }
}