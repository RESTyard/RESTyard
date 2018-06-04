using System.ComponentModel.DataAnnotations;
using WebApi.HypermediaExtensions.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    public class FavoriteCustomer : IHypermediaActionParameter
    {
        [Required]
        [Url]
        public string CustomerLink { get; set; }
    }
}