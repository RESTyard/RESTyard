using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Hypermedia;

public interface IEmbeddedEntity : IRelatedEntity;

public static class EmbeddedEntity
{
    public static EmbeddedEntity<THto> Embed<THto>(THto hto) where THto : IHypermediaObject
        => new EmbeddedEntity<THto>(new HypermediaObjectReference(hto));
}
    
public record EmbeddedEntity<THto>(HypermediaObjectReferenceBase Reference)
    : IEmbeddedEntity
    where THto : IHypermediaObject;