using CarShack.Hypermedia;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;

namespace CarShack.Controllers.EntryPoint
{
    [Route("[controller]/")]
    [ApiController]
    public class EntryPointController : Controller
    {
        private readonly HypermediaEntrypointHto hypermediaEntryPoint;

        public EntryPointController(HypermediaEntrypointHto hypermediaEntryPoint)
        {
            this.hypermediaEntryPoint = hypermediaEntryPoint;
        }

        // Initial route to the API. References to HypermediaEntryPoint type will be resolved to this route.
        // Also an optional name is given to the route for debugging.
        [HttpGetHypermediaObject("", typeof(HypermediaEntrypointHto), Name = RouteNames.EntryPoint.Root)]
        public ActionResult GetRootDocument()
        {
            return Ok(hypermediaEntryPoint);
        }
    }
}
