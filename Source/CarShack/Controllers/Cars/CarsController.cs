using CarShack.Hypermedia;
using CarShack.Hypermedia.Cars;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using RESTyard.WebApi.Extensions.Exceptions;
using RESTyard.WebApi.Extensions.WebApi.AttributedRoutes;
using RESTyard.WebApi.Extensions.WebApi.ExtensionMethods;

namespace CarShack.Controllers.Cars
{
    [Route("Cars/")]
    public class CarsController : Controller
    {
        private readonly HypermediaCarsRootHto carsRoot;

        public CarsController(HypermediaCarsRootHto carsRoot)
        {
            this.carsRoot = carsRoot;
        }
        
        [HttpGetHypermediaObject("", typeof(HypermediaCarsRootHto))]
        public ActionResult GetRootDocument()
        {
            return Ok(carsRoot);
        }

        // example route with more than one placeholder variable. Mapping of object keys to those parameters when creating links
        // is handled by using KeyAttribute on HypermediaCar instead of passing RouteKeyProducer type in HttpGetHypermediaObject attribute.
        [HttpGetHypermediaObject("{brand}/{id:int}", typeof(HypermediaCarHto))]
        public ActionResult GetEntity(string brand, int id)
        {
            try
            {
                // short cut for example, we should actually call the Car repo and get a Car domain object
                var result = new HypermediaCarHto(brand, id);
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
        }
    }
}
