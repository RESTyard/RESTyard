using System.ComponentModel.DataAnnotations;
using CarShack.Hypermedia.Cars;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.JsonSchema;

namespace CarShack.Hypermedia.Customers
{
    public class HypermediaActionCustomerBuysCar : HypermediaAction<HypermediaActionCustomerBuysCar.Parameter>
    {
        public HypermediaActionCustomerBuysCar() : base(() => true)
        {
        }

        public class Parameter : IHypermediaActionParameter
        {
            [KeyFromUri(typeof(HypermediaCar), schemaProperyName: "CarUri", routeTemplateParameterName: "brand")]
            public string Brand { get; set; }
            [KeyFromUri(typeof(HypermediaCar), schemaProperyName: "CarUri", routeTemplateParameterName: "key")]
            public int CarId { get; set; }
            [Required]
            public double Price { get; set; }
        }
    }
}