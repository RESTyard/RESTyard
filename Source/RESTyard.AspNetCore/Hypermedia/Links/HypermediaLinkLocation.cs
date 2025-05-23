using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace RESTyard.AspNetCore.Hypermedia.Links;

public class HypermediaLinkLocation : ActionResult
{
    public HypermediaLinkLocation(ILink link, HttpStatusCode httpStatusCode)
    {
        this.Link = link;
        this.HttpStatusCode = httpStatusCode;
    }
    
    public ILink Link { get; }

    public HttpStatusCode HttpStatusCode { get; }
}