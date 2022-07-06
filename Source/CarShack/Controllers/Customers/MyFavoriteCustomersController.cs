using System.Threading.Tasks;
using CarShack.Domain.Customer;
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
    }
}