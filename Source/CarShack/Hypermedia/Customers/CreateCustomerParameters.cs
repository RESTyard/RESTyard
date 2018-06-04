using System.ComponentModel.DataAnnotations;
using WebApi.HypermediaExtensions.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    public class CreateCustomerParameters : IHypermediaActionParameter
    {
        [Required]
        public string Name { get; set; }
    }
}