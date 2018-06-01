using CarShack.Hypermedia.Customers;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace CarShack.Controllers.Customers
{
    // Generates the key used for a HypermediaCustomer. Required for Routes which have a key. See CustomerController route to HypermediaCustomer type.
    // Is internaly passed to a UrlHelper.
    public class CustomerRouteKeyProducer : IKeyProducer
    {
        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            var customer = hypermediaObject as HypermediaCustomer;
            if (customer == null)
            {
                throw new HypermediaException($"Passed object is not a {typeof(HypermediaCustomer)}");
            }

            // it is required by Web Api to return a anonymous object with a property named exactly as in teh route template and the method argument.
            return new { key = customer.Id };
        }

        public object CreateFromKeyObject(object keyObject)
        {
            return new { key = keyObject };
        }
    }
}