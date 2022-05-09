using System;
using System.Net.Http;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public static class SystemNetHttpExtensions
    {
        public static IHypermediaResolver CreateHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            HttpClient httpClient)
        {
            var dependencies = builder.BuildDependencies();
            var resolver = new HttpHypermediaResolver(
                httpClient,
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                NoLinkCache<string>.Instance);
            return resolver;
        }

        public static Func<HttpClient, IHypermediaResolver> CreateHttpHypermediaResolverFactory(
            this HypermediaClientBuilder builder)
        {
            var dependencies = builder.BuildDependencies();
            return httpClient =>
            {
                var resolver = new HttpHypermediaResolver(
                    httpClient,
                    dependencies.HypermediaReader,
                    dependencies.ParameterSerializer,
                    dependencies.ProblemReader,
                    NoLinkCache<string>.Instance);
                return resolver;
            };
        }

        public static IHypermediaResolver CreateCachedHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            HttpClient httpClient,
            ILinkHcoCache<string> linkHcoCache)
        {
            var dependencies = builder.BuildDependencies();
            var resolver = new HttpHypermediaResolver(
                httpClient,
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                linkHcoCache);
            return resolver;
        }

        public static Func<HttpClient, IHypermediaResolver> CreatedCachedHttpHypermediaResolverFactory(
            this HypermediaClientBuilder builder,
            ILinkHcoCache<string> linkHcoCache)
        {
            var dependencies = builder.BuildDependencies();
            return httpClient =>
            {
                var resolver = new HttpHypermediaResolver(
                    httpClient,
                    dependencies.HypermediaReader,
                    dependencies.ParameterSerializer,
                    dependencies.ProblemReader,
                    linkHcoCache);
                return resolver;
            };
        }
    }
}
