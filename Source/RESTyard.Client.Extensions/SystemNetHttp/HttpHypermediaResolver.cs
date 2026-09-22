using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunicularSwitch;
using RESTyard.Client.Exceptions;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver;
using RESTyard.Client.Resolver.Caching;
using RESTyard.MediaTypes;

namespace RESTyard.Client.Extensions.SystemNetHttp
{
    public class HttpHypermediaResolver
        : HypermediaResolverBase<
            HttpResponseMessage,
            MultipartFormDataContent,
            HttpLinkHcoCacheEntry,
            HttpLinkHcoCacheEntryConfiguration>
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

        protected override async Task<HypermediaResult<CacheEntryVerificationResult<HttpResponseMessage>>> VerifyIfCacheEntryCanBeUsedAsync(
            Uri uriToResolve,
            HttpLinkHcoCacheEntry cacheEntry,
            DateTimeOffset assumedNow,
            bool forceResolve,
            CancellationToken cancellationToken = default)
        {
            bool forceRevalidate = forceResolve;
            var mustRevalidate = forceRevalidate || cacheEntry.IsRevalidationRequired(assumedNow);
            if (!mustRevalidate)
            {
                return CacheEntryVerificationResult<HttpResponseMessage>.CacheEntryMayBeUsed();
            }

            var request = CreateRevalidationRequest(uriToResolve, cacheEntry);
            var webResult = await HypermediaResult.Try(
                async () => await this.httpClient.SendAsync(request, cancellationToken),
                HypermediaProblem.Exception);
            return webResult
                .Map(response =>
                {
                    var assumedNowAfterRequest = DateTimeOffset.Now;
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        this.UpdateCacheEntry(
                            uriToResolve: uriToResolve,
                            oldEntry: cacheEntry,
                            newConfiguration: HttpLinkHcoCacheEntryConfiguration.FromHttpResponse(response, assumedNowAfterRequest));
                        return CacheEntryVerificationResult<HttpResponseMessage>.CacheEntryMayBeUsed();
                    }

                    return CacheEntryVerificationResult<HttpResponseMessage>.UseThisResponseInstead(response);
                });
        }

        protected override HttpLinkHcoCacheEntryConfiguration GetCacheConfigurationFromResponse(
            HttpResponseMessage response,
            DateTimeOffset assumedNow)
        {
            return HttpLinkHcoCacheEntryConfiguration.FromHttpResponse(response, assumedNow);
        }

        protected override HttpLinkHcoCacheEntry GetCacheEntryFromConfiguration(
            string linkResponseContent,
            HttpLinkHcoCacheEntryConfiguration cacheConfiguration)
        {
            return HttpLinkHcoCacheEntry.FromConfiguration(linkResponseContent, cacheConfiguration);
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
            HttpLinkHcoCacheEntry oldEntry,
            HttpLinkHcoCacheEntryConfiguration newConfiguration)
        {
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
            HttpLinkHcoCacheEntryConfiguration newEntryConfiguration)
        {
            return !previousEntry.IsConfigurationEquivalentTo(newEntryConfiguration);
        }

        protected override async Task<HypermediaResult<MultipartFormDataContent>> CreateUploadPayload(
            IHypermediaFileUploadParameter parameterObject,
            ParameterDescription? parameterDescription = null,
            CancellationToken cancellationToken = default)
        {
            MultipartFormDataContent? result = null;
            try
            {
                result = new MultipartFormDataContent();
                if (parameterObject.ParameterObject is not null && parameterDescription is not null)
                {
                    var serializeParameterObject = this.ParameterSerializer.SerializeParameterObject(
                        parameterDescription.Name,
                        parameterObject.ParameterObject);
                    result.Add(
                        new StringContent(serializeParameterObject), parameterDescription.Name);
                }

                foreach (var file in parameterObject.FileDefinitions)
                {
                    result.Add(new StreamContent(await file.OpenReadStreamAsync(cancellationToken)), file.Name, file.FileName);
                }

                return result;
            }
            catch (Exception e)
            {
                result?.Dispose();
                return HypermediaResult.Error(HypermediaProblem.Exception(e));
            }
        }

        protected override async Task<HypermediaResult<HttpResponseMessage>> ResolveAsync(
            Uri uriToResolve,
            CancellationToken cancellationToken = default)
            => await HypermediaResult.Try(
                async () => await this.httpClient.GetAsync(uriToResolve, cancellationToken),
                HypermediaProblem.Exception);

        protected override async Task<HypermediaResult<HttpResponseMessage>> SendCommandAsync(
            Uri uri,
            string method,
            string? payload = null,
            CancellationToken cancellationToken = default)
        {
            var httpMethod = GetHttpMethod(method);
            using var request = new HttpRequestMessage(httpMethod, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, DefaultMediaTypes.ApplicationJson);//CONTENT-TYPE header    
            }

            return await HypermediaResult.Try(
                async () => await httpClient.SendAsync(request, cancellationToken),
                HypermediaProblem.Exception);
        }

        protected override async Task<HypermediaResult<HttpResponseMessage>> SendUploadCommandAsync(
            Uri uri,
            string method,
            MultipartFormDataContent uploadPayload,
            CancellationToken cancellationToken = default)
        {
            using var _ = uploadPayload;

            var httpMethod = GetHttpMethod(method);
            using var request = new HttpRequestMessage(httpMethod, uri);
            request.Content = uploadPayload;

            return await HypermediaResult.Try(
                async () => await httpClient.SendAsync(request, cancellationToken),
                HypermediaProblem.Exception);
        }

        protected override async Task<HypermediaResult<Unit>> EnsureRequestIsSuccessfulAsync(
            HttpResponseMessage responseMessage,
            CancellationToken cancellationToken = default)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return HypermediaResult.Ok(No.Thing);
            }

            var problemDescriptionOption = await this.TryReadProblemStringAsync(responseMessage, cancellationToken);
            return HypermediaResult.Error(
                problemDescriptionOption.Match(
                    some: problemDescription =>
                    {
                        problemDescription.Status ??= (int)responseMessage.StatusCode;
                        return HypermediaProblem.ProblemDetails(problemDescription);
                    },
                    none: () => HypermediaProblem.StatusCode((int)responseMessage.StatusCode)));


        }

        private async Task<Option<ProblemDetails>> TryReadProblemStringAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken = default)
        {
            if (response.Content == null)
            {
                return Option.None();
            }
            try
            {
#if NET8_0_OR_GREATER
                var contentAsString = await response.Content.ReadAsStringAsync(cancellationToken);
#else
                cancellationToken.ThrowIfCancellationRequested();
                var contentAsString = await response.Content.ReadAsStringAsync();
#endif
                return this.ProblemReader.TryReadProblemString(contentAsString);
            }
            catch (Exception)
            {
                return Option.None();
            }
        }

        protected override async Task<HypermediaResult<Stream>> ResponseAsStreamAsync(
            HttpResponseMessage responseMessage,
            CancellationToken cancellationToken = default)
            => await HypermediaResult.Try(
                async () =>
                {
#if NET8_0_OR_GREATER
                    return await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
#else
                    cancellationToken.ThrowIfCancellationRequested();
                    return await responseMessage.Content.ReadAsStreamAsync();
#endif
                },
                HypermediaProblem.Exception);

        protected override HypermediaResult<Uri> GetLocation(HttpResponseMessage responseMessage)
        {
            var location = responseMessage.Headers.Location;
            if (location is null)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidResponse("hypermedia function did not return a result resource location."));
            }
            return HypermediaResult.Ok(location);
        }

        protected override bool WasFunctionResultInlined(HttpResponseMessage responseMessage, [NotNullWhen(true)] out Uri? locationOfInlinedResult)
        {
            if (responseMessage.StatusCode is HttpStatusCode.OK
                && responseMessage.Headers.TryGetValues("Content-Location", out var values))
            {
                var value = values.FirstOrDefault();
                if (value is not null)
                {
                    locationOfInlinedResult = new Uri(value);
                    return true;
                }
            }

            locationOfInlinedResult = null;
            return false;
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
                case "QUERY":
                    return new HttpMethod("QUERY");
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