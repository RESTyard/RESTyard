using System;
using System.Net.Http;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Extensions.SystemNetHttp
{
    public class HttpHypermediaResolverFactory : IHttpHypermediaResolverFactory
    {
        private readonly IHypermediaReader hypermediaReader;
        private readonly IParameterSerializer parameterSerializer;
        private readonly IProblemStringReader problemReader;
        private readonly ILinkHcoCache<HttpLinkHcoCacheEntry> linkHcoCache;

        public HttpHypermediaResolverFactory(
            IHypermediaReader hypermediaReader,
            IParameterSerializer parameterSerializer,
            IProblemStringReader problemReader,
            ILinkHcoCache<HttpLinkHcoCacheEntry> linkHcoCache)
        {
            this.hypermediaReader = hypermediaReader;
            this.parameterSerializer = parameterSerializer;
            this.problemReader = problemReader;
            this.linkHcoCache = linkHcoCache;
        }

        public IHypermediaResolver Create(
            HttpClient httpClient,
            bool disposeHttpClient = false)
        {
            return new HttpHypermediaResolver(
                httpClient,
                disposeHttpClient,
                this.hypermediaReader,
                this.parameterSerializer,
                this.problemReader,
                this.linkHcoCache);
        }
    }

    public class HttpHypermediaResolverFactory<TParameter> : IHttpHypermediaResolverFactory<TParameter>
    {
        private readonly IHypermediaReader hypermediaReader;
        private readonly IParameterSerializer parameterSerializer;
        private readonly IProblemStringReader problemReader;
        private readonly Func<TParameter, ILinkHcoCache<HttpLinkHcoCacheEntry>> createLinkHcoCache;

        public HttpHypermediaResolverFactory(
            IHypermediaReader hypermediaReader,
            IParameterSerializer parameterSerializer,
            IProblemStringReader problemReader,
            Func<TParameter, ILinkHcoCache<HttpLinkHcoCacheEntry>> createLinkHcoCache)
        {
            this.hypermediaReader = hypermediaReader;
            this.parameterSerializer = parameterSerializer;
            this.problemReader = problemReader;
            this.createLinkHcoCache = createLinkHcoCache;
        }

        public IHypermediaResolver Create(
            HttpClient httpClient,
            TParameter parameter,
            bool disposeHttpClient = false)
        {
            return new HttpHypermediaResolver(
                httpClient,
                disposeHttpClient,
                this.hypermediaReader,
                this.parameterSerializer,
                this.problemReader,
                this.createLinkHcoCache(parameter));
        }
    }
}