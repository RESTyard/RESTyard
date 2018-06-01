using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.Hypermedia.Links;

namespace WebApi.HypermediaExtensions.Hypermedia
{
    public class HypermediaEntityLocation : ActionResult
    {
        public HttpStatusCode HttpStatusCode { get; private set; }

        public HypermediaEntityLocation(HypermediaObjectReferenceBase entityRef, HttpStatusCode httpStatusCode)
        {
            this.HttpStatusCode = httpStatusCode;
            this.EntityRef = entityRef;
        }

        public HypermediaObjectReferenceBase EntityRef { get; set; }
    }
}