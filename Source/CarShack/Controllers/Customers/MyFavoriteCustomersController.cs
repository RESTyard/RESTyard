using CarShack.Domain.Customer;
using CarShack.Hypermedia.Customers;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult FavoriteCustomerType()
        {
            var schema = JsonSchemaFactory.Generate(typeof(FavoriteCustomer));

            return Ok(schema);
        }
        #endregion
    }
}