using System;

namespace RESTyard.Client.Extensions.SystemNetHttp
{
    public enum CacheMode
    {
        Undefined,
        NoRevalidationRequired,
        AlwaysRevalidate,
        RevalidateStale,
        DoNotCache,
    }
}