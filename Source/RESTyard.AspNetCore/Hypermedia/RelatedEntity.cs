using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Hypermedia;

/// <summary>
/// A Entity which is related to a HypermediaObject.
/// </summary>
public interface IRelatedEntity
{
    /// <summary>
    /// A reference to the related entity.
    /// </summary>
    public HypermediaObjectReferenceBase Reference { get; }
}