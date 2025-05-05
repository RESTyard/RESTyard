using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Hypermedia;

public interface IEmbeddedEntity : IRelatedEntity;

public interface IEmbeddedEntity<out THto> : IEmbeddedEntity;

public static class EmbeddedEntity
{
    public static IEmbeddedEntity<THto> Embed<THto>(THto hto) where THto : IHypermediaObject
        => new EmbeddedEntity<THto>(new HypermediaObjectReference(hto));
}
    
public record EmbeddedEntity<THto>(HypermediaObjectReferenceBase Reference)
    : IEmbeddedEntity<THto>
    where THto : IHypermediaObject;