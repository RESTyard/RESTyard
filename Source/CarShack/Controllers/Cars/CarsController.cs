using CarShack.Hypermedia.Cars;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes;
using WebApiHypermediaExtensionsCore.WebApi.ExtensionMethods;

namespace CarShack.Controllers.Cars
{
    [Route("Cars/")]
    public class CarsController : Controller
    {
        private readonly HypermediaCarsRoot carsRoot;

        public CarsController(HypermediaCarsRoot carsRoot)
        {
            this.carsRoot = carsRoot;
        }
        
        [HttpGetHypermediaObject("", typeof(HypermediaCarsRoot))]
        public ActionResult GetRootDocument()
        {
            return Ok(carsRoot);
        }

        // example route with more than one placeholder variable
        [HttpGetHypermediaObject("{brand}/{key:int}", typeof(HypermediaCar), typeof(CarRouteKeyProducer))]
        public ActionResult GetEntity(string brand, int key)
        {
            try
            {
                // short cut for example, we should actually call the Car repo and get a Car domain object
                var result = new HypermediaCar(brand, key);
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
        }
    }
}
