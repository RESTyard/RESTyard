using System;
using FunicularSwitch;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.WebApi.RouteResolver;

public interface IKeyFromUriService
{
    /// <summary>
    /// Extracts the hypermedia key from the rendered Uri.
    /// Expects <typeparamref name="TKey"/> to be a record with members named exactly like the key properties
    /// </summary>
    /// <typeparam name="THto">The hypermedia object type, to resolve the correct route template</typeparam>
    /// <typeparam name="TKey">The key type, to resolve the correct parameter types and to return a typed result</typeparam>
    /// <param name="uri">The Uri</param>
    /// <returns>The key created from the values in the Uri</returns>
    Result<TKey> GetKeyFromUri<THto, TKey>(Uri uri)
        where THto : HypermediaObject;
}