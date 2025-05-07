using System;

namespace RESTyard.AspNetCore.Hypermedia;

[Obsolete("HTOs should no longer derive from HypermediaObject, but rather be marked with IHypermediaObject and define links and entities explicitly")]
public abstract class HypermediaObject : IHypermediaObject
{
    
}