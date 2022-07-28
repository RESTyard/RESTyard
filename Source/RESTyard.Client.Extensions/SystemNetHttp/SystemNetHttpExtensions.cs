using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using RESTyard.Client.Authentication;
using RESTyard.Client.Builder;
using RESTyard.Client.Resolver;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Extensions.SystemNetHttp
{
    public static class SystemNetHttpExtensions
    {
        /// <summary>
        /// Create an IHypermediaResolver that communicates with the Server via HTTP using the given HttpClient.
        /// </summary>
        /// <param name="builder">The HypermediaResolverBuilder</param>
        /// <param name="httpClient">The HttpClient to use for the network communication. Set up headers like Authentication beforehand.</param>
        /// <param name="disposeHttpClient">If <c>true</c>, disposes the injected HttpClient when the IHypermediaResolver is disposed</param>
        /// <returns></returns>
        public static IHypermediaResolver CreateHttpHypermediaResolver(
            this IHypermediaResolverBuilder builder,
            HttpClient httpClient,
            bool disposeHttpClient = true)
        {
            var dependencies = builder.BuildDependencies();
            var resolver = new HttpHypermediaResolver(
                httpClient,
                disposeHttpClient,
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                NoLinkCache<HttpLinkHcoCacheEntry>.Instance);
            return resolver;
        }

        /// <summary>
        /// Create a factory to build an IHypermediaResolver that communicates with the server via HTTP. The HttpClient used for the network communication is provided as a parameter to the factory method.
        /// </summary>
        /// <param name="builder">The HypermediaResolverBuilder</param>
        /// <returns></returns>
        public static IHttpHypermediaResolverFactory CreateHttpHypermediaResolverFactory(
            this IHypermediaResolverBuilder builder)
        {
            var dependencies = builder.BuildDependencies();
            return new HttpHypermediaResolverFactory(
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                NoLinkCache<HttpLinkHcoCacheEntry>.Instance);
        }

        /// <summary>
        /// Create an IHypermediaResolver that communicates with the Server via HTTP using the given HttpClient, and with the ability to cache the results of HypermediaLinks
        /// </summary>
        /// <param name="builder">The HypermediaResolverBuilder</param>
        /// <param name="httpClient">The HttpClient to use for the network communication. Set up headers like Authentication beforehand.</param>
        /// <param name="linkHcoCache">The cache to store and retrieve results of HypermediaLinks.</param>
        /// <param name="disposeHttpClient">If <c>true</c>, disposes the injected HttpClient when the IHypermediaResolver is disposed</param>
        /// <returns></returns>
        public static IHypermediaResolver CreateCachedHttpHypermediaResolver(
            this IHypermediaResolverBuilder builder,
            HttpClient httpClient,
            ILinkHcoCache<HttpLinkHcoCacheEntry> linkHcoCache,
            bool disposeHttpClient = true)
        {
            var dependencies = builder.BuildDependencies();
            var resolver = new HttpHypermediaResolver(
                httpClient,
                disposeHttpClient,
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                linkHcoCache);
            return resolver;
        }

        /// <summary>
        /// Create a factory to build an IHypermediaResolver that communicates with the server via HTTP, with the ability to cache the results of HypermediaLinks. The HttpClient used for the network communication is provided as a parameter to the factory method.
        /// </summary>
        /// <param name="builder">The HypermediaResolverBuilder</param>
        /// <param name="linkHcoCache">The cache to store and retrieve results of HypermediaLinks</param>
        /// <returns></returns>
        public static IHttpHypermediaResolverFactory CreatedCachedHttpHypermediaResolverFactory(
            this IHypermediaResolverBuilder builder,
            ILinkHcoCache<HttpLinkHcoCacheEntry> linkHcoCache)
        {
            var dependencies = builder.BuildDependencies();
            return new HttpHypermediaResolverFactory(
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                linkHcoCache);
        }

        /// <summary>
        /// Create a factory to build an IHypermediaResolver that communicates with the server via HTTP, with the ability to cache the results of HypermediaLinks per session. The HttpClient used for the network communication is provided as a parameter to the factory method, as well as an additional parameter used for creating the cache.
        /// </summary>
        /// <typeparam name="TParameter">The Parameter type to instantiate the cache</typeparam>
        /// <param name="builder">The IHypermediaResolverBuilder</param>
        /// <param name="createLinkHcoCache">The factory to create the cache that stores and retrieves the results of HypermediaLinks</param>
        /// <returns></returns>
        public static IHttpHypermediaResolverFactory<TParameter> CreateCachedHttpHypermediaResolverFactory<TParameter>(
            this IHypermediaResolverBuilder builder,
            Func<TParameter, ILinkHcoCache<HttpLinkHcoCacheEntry>> createLinkHcoCache)
        {
            var dependencies = builder.BuildDependencies();
            return new HttpHypermediaResolverFactory<TParameter>(
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                createLinkHcoCache);
        }

        public static AuthenticationHeaderValue CreateBasicAuthHeaderValue(
            this UsernamePasswordCredentials credentials)
        {
            var encodedCredentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials.User + ":" + credentials.Password));
            return new AuthenticationHeaderValue("Basic", encodedCredentials);
        }
    }
}
