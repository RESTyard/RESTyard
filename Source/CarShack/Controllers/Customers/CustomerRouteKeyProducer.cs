using CarShack.Hypermedia;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace CarShack.Controllers.Customers
{
    // Generates the key used for a HypermediaCustomer. Required for routes which have a key. See CustomerController route to HypermediaCustomer type.
    // Is internaly passed to a UrlHelper.

    // This is an exlicit key producer implementation. This is only necessary if something special should be done to match url template parameters to entity keys. 
    // If no explicit key producer implementation is provided in HttpGetHypermediaObject attribute a key producer is registered automatically at 
    // application startup. For automatic registration use KeyAttribute on property Id of HypermediaCustomer. Look at HypermediaCar uses implicit KeyProducers.
    public class CustomerRouteKeyProducer : IKeyProducer
    {
        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            if (!(hypermediaObject is HypermediaCustomerHto customer))
            {
                throw new HypermediaException($"Passed object is not a {typeof(HypermediaCustomerHto)}");
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