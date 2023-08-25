using System;
using System.Net.Http;
using RESTyard.Client.Resolver;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Extensions.SystemNetHttp
{
    /// <summary>
    /// Factory to create a pre-configured IHypermediaResolver for immediate use
    /// </summary>
    public interface IHttpHypermediaResolverFactory
    {
        /// <summary>
        /// Create an IHypermediaResolver that uses the given HttpClient to communicate.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use for network communication. Set up headers like Authentication before resolving.</param>
        /// <param name="disposeHttpClient">If <c>true</c>, disposes the HttpClient when the IHypermediaResolver is disposed.</param>
        /// <returns></returns>
        IHypermediaResolver Create(
            HttpClient httpClient,
            bool disposeHttpClient = false);
    }

    /// <summary>
    /// Factory to create a pre-configured IHypermediaResolver for immediate use
    /// </summary>
    /// <typeparam name="TParameter">Parameter to customize the creation of the <see cref="ILinkHcoCache{TLinkHcoCacheEntry}" /></typeparam>
    public interface IHttpHypermediaResolverFactory<in TParameter>
    {
        /// <summary>
        /// Create an IHypermediaResolver that uses the given HttpClient to communicate.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use for network communication. Set up headers like Authentication before resolving.</param>
        /// <param name="parameter">A parameter to pass when creating the <see cref="ILinkHcoCache{TLinkHcoCacheEntry}" /></param>
        /// <param name="disposeHttpClient">If <c>true</c>, disposes the HttpClient when the IHypermediaResolver is disposed.</param>
        /// <returns></returns>
        IHypermediaResolver Create(
            HttpClient httpClient,
            TParameter parameter,
            bool disposeHttpClient = false);
    }
}