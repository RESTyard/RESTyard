using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Bluehands.Hypermedia.Client.Authentication;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public static class SystemNetHttpExtensions
    {
        public static IHypermediaResolver CreateHttpHypermediaResolver(
            this HypermediaResolverBuilder builder,
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
            this HypermediaResolverBuilder builder)
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
            this HypermediaResolverBuilder builder,
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
            this HypermediaResolverBuilder builder,
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

        public static AuthenticationHeaderValue CreateBasicAuthHeaderValue(
            this UsernamePasswordCredentials credentials)
        {
            var encodedCredentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials.User + ":" + credentials.Password));
            return new AuthenticationHeaderValue("Basic", encodedCredentials);
        }
    }
}
