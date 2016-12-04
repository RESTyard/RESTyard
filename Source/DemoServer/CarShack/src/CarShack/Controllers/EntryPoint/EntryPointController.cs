using CarShack.Hypermedia.EntryPoint;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes;

namespace CarShack.Controllers.EntryPoint
{
    [Route("[controller]/")]
    public class EntryPointController : Controller
    {
        private readonly HypermediaEntryPoint hypermediaEntryPoint;

        public EntryPointController(HypermediaEntryPoint hypermediaEntryPoint)
        {
            this.hypermediaEntryPoint = hypermediaEntryPoint;
        }

        // Initial route to the API. References to HypermediaEntryPoint type will be resolved to this route.
        // Also an optional name is given to the route for debugging.
        [HttpGetHypermediaObject("", typeof(HypermediaEntryPoint), Name = RouteNames.EntryPoint.Root)]
        public ActionResult GetRootDocument()
        {
            return Ok(hypermediaEntryPoint);
        }
    }
}
