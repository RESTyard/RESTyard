using System.Net.Http;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
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
}