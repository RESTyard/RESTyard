using System.ComponentModel.DataAnnotations;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace CarShack.Hypermedia.Customers
{
    public class FavoriteCustomer : IHypermediaActionParameter
    {
        [Required]
        [KeyFromUri(typeof(HypermediaCustomer), schemaProperyName: "Customer", routeTemplateParameterName: null)]
        public int CustomerId { get; set; }
    }
}