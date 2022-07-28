using System;

namespace RESTyard.Client.Resolver.Caching
{
    public enum CacheScope
    {
        Undefined,
        AcrossUserContexts,
        ForIndividualUserContext,
    }
}