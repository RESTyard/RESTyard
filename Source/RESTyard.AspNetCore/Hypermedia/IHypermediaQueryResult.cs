using RESTyard.AspNetCore.Query;

namespace RESTyard.AspNetCore.Hypermedia;

public interface IHypermediaQueryResult : IHypermediaObject
{
    IHypermediaQuery Query { get; }
}