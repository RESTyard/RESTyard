using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Exceptions;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver;
using Bluehands.Hypermedia.Client.Resolver.Caching;
using Bluehands.Hypermedia.MediaTypes;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public class HttpHypermediaResolver
        : HypermediaResolverBase<HttpResponseMessage, HttpLinkHcoCacheEntry>
    {
        private readonly HttpClient httpClient;
        private readonly bool disposeHttpClient;

        private bool alreadyDisposed = false;

        public HttpHypermediaResolver(
            HttpClient httpClient,
            bool disposeHttpClient,
            IHypermediaReader hypermediaReader,
            IParameterSerializer parameterSerializer,
            IProblemStringReader problemReader,
            ILinkHcoCache<HttpLinkHcoCacheEntry> linkHcoCache)
            : base(hypermediaReader, parameterSerializer, problemReader, linkHcoCache)
        {
            this.httpClient = httpClient;
            this.disposeHttpClient = disposeHttpClient;
        }

        public override async Task<ResolverResult<T>> ResolveLinkAsync<T>(
            Uri uriToResolve,
            bool forceResolve = false)
        {
            HttpResponseMessage response;
            bool forceRevalidate = forceResolve;
            if (this.LinkHcoCache.TryGetValue(uriToResolve, out var cacheEntry))
            {
                var mustRevalidate = forceRevalidate || cacheEntry.IsRevalidationRequired(DateTimeOffset.Now);
                if (!mustRevalidate)
                {
                    var hco = this.HypermediaReader.Read(cacheEntry.LinkResponseContent);
                    return new ResolverResult<T>(true, (T)hco, this);
                }
                var request = CreateRevalidationRequest(uriToResolve, cacheEntry);
                response = await this.httpClient.SendAsync(request, CancellationToken.None);
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    this.UpdateCacheEntry(uriToResolve, response, cacheEntry);

                    var hco = this.HypermediaReader.Read(cacheEntry.LinkResponseContent);
                    return new ResolverResult<T>(true, (T)hco, this);
                }
                else
                {
                    this.LinkHcoCache.Remove(uriToResolve);
                }
            }
            else
            {
                response = await httpClient.GetAsync(uriToResolve);
            }

            var cacheConfiguration = CacheEntryConfiguration.FromHttpResponse(response, DateTimeOffset.Now);
            bool exportToString = cacheConfiguration.ShouldBeAddedToCache();
            var (resolverResult, hcoAsString) = await HandleLinkResponseAsync<T>(response, exportToString);

            if (resolverResult.Success && exportToString && !string.IsNullOrEmpty(hcoAsString))
            {
                var entry = HttpLinkHcoCacheEntry.FromConfiguration(hcoAsString, cacheConfiguration);
                this.LinkHcoCache.Set(uriToResolve, entry);
            }

            return resolverResult;
        }

        private static HttpRequestMessage CreateRevalidationRequest(
            Uri uriToResolve,
            HttpLinkHcoCacheEntry cacheEntry)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = uriToResolve,
                Method = HttpMethod.Get,
            };
            if (!string.IsNullOrEmpty(cacheEntry.ETag))
            {
                request.Headers.IfNoneMatch.Add(
                    new EntityTagHeaderValue(StringHelpers.SurroundWithQuotes(cacheEntry.ETag)));
            }
            if (cacheEntry.LastModified != null)
            {
                request.Headers.IfModifiedSince = cacheEntry.LastModified;
            }

            return request;
        }

        private void UpdateCacheEntry(
            Uri uriToResolve,
            HttpResponseMessage response,
            HttpLinkHcoCacheEntry oldEntry)
        {
            var newConfiguration = CacheEntryConfiguration.FromHttpResponse(response, DateTimeOffset.Now);
            if (!newConfiguration.HasCacheConfiguration)
            {
                //TODO generate warning
                this.LinkHcoCache.Remove(uriToResolve);
            }
            else if (!newConfiguration.ShouldBeAddedToCache())
            {
                this.LinkHcoCache.Remove(uriToResolve);
            }
            else if (HasUpdatedCacheConfiguration(oldEntry, newConfiguration))
            {
                this.LinkHcoCache.Replace(
                    uriToResolve,
                    oldEntry,
                    HttpLinkHcoCacheEntry.FromConfiguration(oldEntry.LinkResponseContent, newConfiguration));
            }
        }

        private static bool HasUpdatedCacheConfiguration(
            HttpLinkHcoCacheEntry previousEntry,
            CacheEntryConfiguration newEntryConfiguration)
        {
            return !previousEntry.IsConfigurationEquivalentTo(newEntryConfiguration);
        }

        protected override async Task<HttpResponseMessage> SendCommandAsync(
            Uri uri,
            string method,
            string payload = null)
        {
            var httpMethod = GetHttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, DefaultMediaTypes.ApplicationJson);//CONTENT-TYPE header    
            }

            var responseMessage = await httpClient.SendAsync(request);

            return responseMessage;
        }

        protected override async Task EnsureRequestIsSuccessfulAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return;
            }

            var innerException = GetInnerException(responseMessage);

            var (hasProblemDescription, problemDescription) = await this.TryReadProblemStringAsync(responseMessage);
            if (hasProblemDescription)
            {
                throw new HypermediaProblemException(problemDescription, innerException);
            }

            var message = innerException.Message ?? string.Empty;
            throw new RequestNotSuccessfulException(message, (int)responseMessage.StatusCode, innerException);
        }

        private async Task<(bool hasProblemDescription, ProblemDescription problemDescription)> TryReadProblemStringAsync(HttpResponseMessage response)
        {
            if (response.Content == null)
            {
                return (false, null);
            }
            try
            {
                var contentAsString = await response.Content.ReadAsStringAsync();
                var result = this.ProblemReader.TryReadProblemString(contentAsString, out var problemDescription);
                return (result, problemDescription);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        private static Exception GetInnerException(HttpResponseMessage result)
        {
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception inner)
            {
                return inner;
            }

            return null;
        }

        protected override async Task<Stream> ResponseAsStreamAsync(HttpResponseMessage responseMessage)
        {
            return await responseMessage.Content.ReadAsStreamAsync();
        }

        protected override Uri GetLocation(HttpResponseMessage responseMessage)
        {
            return responseMessage.Headers.Location;
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            switch (method.ToUpperInvariant())
            {
                case "DELETE":
                    return HttpMethod.Delete;
                case "GET":
                    return HttpMethod.Get;
                case "HEAD":
                    return HttpMethod.Head;
                case "OPTIONS":
                    return HttpMethod.Options;
                case "POST":
                    return HttpMethod.Post;
                case "PUT":
                    return HttpMethod.Put;
                case "TRACE":
                    return HttpMethod.Trace;
                case "PATCH":
                    return new HttpMethod("PATCH");
                default:
                    throw new Exception($"Unknown method: '{method}'");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.alreadyDisposed)
            {
                return;
            }
            if (disposing)
            {
                if (this.disposeHttpClient)
                {
                    this.httpClient.Dispose();
                }
            }

            this.alreadyDisposed = true;
            base.Dispose(disposing);
        }
    }
}