using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Hypermedia;

/// <summary>
/// Base class for records that describe the key properties for an <see cref="IHypermediaObject"/>.
/// Any Key record inheriting from <see cref="HypermediaObjectKeyBase{THto}"/> can be passed to a <see cref="LinkGenerator"/> as the values parameter and will be used to initialize a <see cref="RouteValueDictionary"/>
/// </summary>
/// <typeparam name="THto">The <see cref="IHypermediaObject"/> that this key record describes.</typeparam>
public abstract record HypermediaObjectKeyBase<THto>() : IHypermediaObjectKey<THto> where THto : IHypermediaObject
{
    IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
        => EnumerateKeysForLinkGeneration().GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator()
        => EnumerateKeysForLinkGeneration().GetEnumerator();
    
    /// <summary>
    /// Implement this in an inheriting class by returning the name of the parameter in an HTTP route together with the corresponding value.
    /// </summary>
    /// <returns>The pairs of names and values for all key parameters.</returns>
    protected abstract IEnumerable<KeyValuePair<string, object?>> EnumerateKeysForLinkGeneration();
    
    /// <summary>
    /// Creates a <see cref="HypermediaObjectReferenceBase"/> that describes the reference to a <typeparamref name="THto"/> with the key parameters described by this key record.
    /// </summary>
    /// <returns>The reference.</returns>
    public HypermediaObjectReferenceBase ToHypermediaObjectReference()
    {
        return new HypermediaObjectKeyReference(typeof(THto), this);
    }
}