using System.ComponentModel.DataAnnotations;
using WebApi.HypermediaExtensions.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    public class NewAddress : IHypermediaActionParameter
    {
        [Required]
        public string Address { get; set; }
    }
}