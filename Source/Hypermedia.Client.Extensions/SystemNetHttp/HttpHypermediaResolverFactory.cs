﻿using System.Net.Http;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver;
using Bluehands.Hypermedia.Client.Resolver.Caching;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
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
}