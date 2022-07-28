using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Hypermedia
{
    public class HypermediaEntityLocation : ActionResult
    {
        public HttpStatusCode HttpStatusCode { get; }

        public HypermediaEntityLocation(HypermediaObjectReferenceBase entityRef, HttpStatusCode httpStatusCode)
        {
            this.HttpStatusCode = httpStatusCode;
            this.EntityRef = entityRef;
        }

        public HypermediaObjectReferenceBase EntityRef { get; set; }
    }
}