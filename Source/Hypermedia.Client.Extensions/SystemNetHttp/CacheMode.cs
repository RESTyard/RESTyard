namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
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