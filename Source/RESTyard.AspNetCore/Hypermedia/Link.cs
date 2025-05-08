using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;

namespace RESTyard.AspNetCore.Hypermedia;

public static class Link
{
    public static ILink<THto> To<THto>(THto hto) where THto : IHypermediaObject
        => new Link<THto>(new HypermediaObjectReference(hto));

    public static ILink<THto> ByKey<THto>(IHypermediaObjectKey<THto>? key) where THto : IHypermediaObject
        => new Link<THto>(new HypermediaObjectKeyReference(typeof(THto), key));

    public static ILink<THto> ByQuery<THto>(IHypermediaQuery query, HypermediaObjectKeyBase<THto>? key = null)
        where THto : IHypermediaQueryResult
        => new Link<THto>(new HypermediaObjectQueryReference(typeof(THto), query, key));

    public static ExternalLink External(HypermediaObjectReferenceBase reference)
        => new ExternalLink(reference);
}

public interface ILink : IRelatedEntity;

public interface ILink<out THto> : ILink where THto : IHypermediaObject;

public record Link<THto>(HypermediaObjectReferenceBase Reference)
    : ILink<THto>
    where THto : IHypermediaObject;

public record ExternalLink(HypermediaObjectReferenceBase Reference) : ILink;