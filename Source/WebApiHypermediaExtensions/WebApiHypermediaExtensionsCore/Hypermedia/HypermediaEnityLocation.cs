using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Hypermedia
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