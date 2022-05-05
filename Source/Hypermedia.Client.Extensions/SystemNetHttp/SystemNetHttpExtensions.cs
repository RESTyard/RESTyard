using System;
using System.Net.Http;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public static class SystemNetHttpExtensions
    {
        public static IHypermediaResolver CreateHttpHypermediaResolver(
            this HypermediaClientBuilder builder,
            HttpClient httpClient,
            Action<IHttpHypermediaResolverConfiguration> configure)
        {
            var dependencies = builder.BuildDependencies();
            var resolver = new HttpHypermediaResolver(
                httpClient,
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                NoLinkCache<string>.Instance);
            configure(resolver);
            return resolver;
        }

        public static Func<HttpClient, IHypermediaResolver> CreateHttpHypermediaResolverFactory(
            this HypermediaClientBuilder builder,
            Action<IHttpHypermediaResolverConfiguration> configure)
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
            ILinkHcoCache<string> linkHcoCache,
            Action<IHttpHypermediaResolverConfiguration> configure)
        {
            var dependencies = builder.BuildDependencies();
            var resolver = new HttpHypermediaResolver(
                httpClient,
                dependencies.HypermediaReader,
                dependencies.ParameterSerializer,
                dependencies.ProblemReader,
                linkHcoCache);
            configure(resolver);
            return resolver;
        }

        public static Func<HttpClient, IHypermediaResolver> CreatedCachedHttpHypermediaResolverFactory(
            this HypermediaClientBuilder builder,
            ILinkHcoCache<string> linkHcoCache,
            Action<IHttpHypermediaResolverConfiguration> configure)
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
                configure(resolver);
                return resolver;
            };
        }
    }
}
