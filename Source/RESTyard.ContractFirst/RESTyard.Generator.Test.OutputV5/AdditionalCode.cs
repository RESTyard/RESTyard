using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Query;

namespace RESTyard.Generator.Test.Output;

public record External : IHypermediaActionParameter;

public class HypermediaQueryResult_V5_0 : IHypermediaQueryResult
{
    public IHypermediaQuery Query { get; }

    public HypermediaQueryResult_V5_0(IHypermediaQuery query)
    {
        this.Query = query;
    }
}