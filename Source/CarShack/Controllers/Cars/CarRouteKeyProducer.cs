using CarShack.Hypermedia.Cars;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace CarShack.Controllers.Cars
{
    public class CarRouteKeyProducer : IKeyProducer
    {
        public object CreateFromHypermediaObject(HypermediaObject hypermediaObject)
        {
            var car = hypermediaObject as HypermediaCar;
            if (car == null)
            {
                throw new HypermediaException($"Passed object is not a {typeof(HypermediaCar)}");
            }

            return new { brand = car.Brand, key = car.Id };
        }

        public object CreateFromKeyObject(object keyObject)
        {
            // the passed object is already the desired anaonymous object
            return keyObject;
        }
    }
}