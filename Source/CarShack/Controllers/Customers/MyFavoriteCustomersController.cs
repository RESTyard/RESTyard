using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;

namespace CarShack.Controllers.Customers
{
    [Route("MyFavoriteCustomers")]
    [ApiController]
    public class MyFavoriteCustomersController : Controller
    {
        private readonly ICustomerRepository customerRepository;

        public MyFavoriteCustomersController(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
        }
    }
}