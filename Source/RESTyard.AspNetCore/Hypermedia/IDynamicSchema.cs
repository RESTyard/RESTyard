namespace RESTyard.AspNetCore.Hypermedia;

public interface IDynamicSchema
{
    public object? SchemaRouteKeys { get; set; }
}