using System.ComponentModel.DataAnnotations;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    public class CreateCustomerParameters : IHypermediaActionParameter
    {
        [Required]
        public string Name { get; set; }
    }
}