using System;
using System.IO;
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

        protected override async Task<CacheEntryVerificationResult<HttpResponseMessage>> VerifyIfCacheEntryCanBeUsedAsync(
            Uri uriToResolve,
            HttpLinkHcoCacheEntry cacheEntry,
            DateTimeOffset assumedNow,
            bool forceResolve)
        {
            bool forceRevalidate = forceResolve;
            var mustRevalidate = forceRevalidate || cacheEntry.IsRevalidationRequired(assumedNow);
            if (!mustRevalidate)
            {
                return new CacheEntryVerificationResult<HttpResponseMessage>.CacheEntryMayBeUsed();
            }

            var request = CreateRevalidationRequest(uriToResolve, cacheEntry);
            var response = await this.httpClient.SendAsync(request, CancellationToken.None);
            var assumedNowAfterRequest = DateTimeOffset.Now;
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                this.UpdateCacheEntry(uriToResolve, cacheEntry, HttpLinkHcoCacheEntryConfiguration.FromHttpResponse(response, assumedNowAfterRequest));
                return new CacheEntryVerificationResult<HttpResponseMessage>.CacheEntryMayBeUsed();
            }

            return new CacheEntryVerificationResult<HttpResponseMessage>.UseThisResponseInstead(response);
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
            ParameterDescription? parameterDescription = null)
        {
            MultipartFormDataContent? result = null;
            try
            {
                result = new MultipartFormDataContent();
                if (parameterObject.ParameterObject is not null)
                {
                    var serializeParameterObject = this.ParameterSerializer.SerializeParameterObject(nameof(parameterObject.ParameterObject),
                        parameterObject.ParameterObject);
                    result.Add(
                        new StringContent(serializeParameterObject), nameof(IHypermediaFileUploadParameter.ParameterObject));
                }

                foreach (var file in parameterObject.FileDefinitions)
                {
                    result.Add(new StreamContent(await file.OpenReadStreamAsync()), file.Name, file.FileName);
                }

                return result;
            }
            catch (Exception e)
            {
                result?.Dispose();
                return HypermediaResult.Error<MultipartFormDataContent>(HypermediaProblem.Exception(e));
            }
        }

        protected override async Task<HypermediaResult<HttpResponseMessage>> ResolveAsync(Uri uriToResolve)
        {
            try
            {
                return await this.httpClient.GetAsync(uriToResolve);
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<HttpResponseMessage>(HypermediaProblem.Exception(e));
            }
        }

        protected override async Task<HypermediaResult<HttpResponseMessage>> SendCommandAsync(
            Uri uri,
            string method,
            string? payload = null)
        {
            var httpMethod = GetHttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, DefaultMediaTypes.ApplicationJson);//CONTENT-TYPE header    
            }

            try
            {
                var responseMessage = await httpClient.SendAsync(request);
                return responseMessage;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<HttpResponseMessage>(HypermediaProblem.Exception(e));
            }
        }

        protected override async Task<HypermediaResult<HttpResponseMessage>> SendUploadCommandAsync(
            Uri uri,
            string method,
            MultipartFormDataContent uploadPayload)
        {
            using var _ = uploadPayload;

            var httpMethod = GetHttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, uri);
            request.Content = uploadPayload;

            try
            {
                var responseMessage = await httpClient.SendAsync(request);
                return responseMessage;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<HttpResponseMessage>(HypermediaProblem.Exception(e));
            }
        }

        protected override async Task<HypermediaResult<Unit>> EnsureRequestIsSuccessfulAsync(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                return HypermediaResult.Ok(No.Thing);
            }

            var (hasProblemDescription, problemDescription) = await this.TryReadProblemStringAsync(responseMessage);
            if (hasProblemDescription)
            {
                if (problemDescription!.Status is not null)
                {
                    return HypermediaResult.Error<Unit>(HypermediaProblem.ProblemDetails(problemDescription));
                }
                var problemDetailsCopy = new ProblemDetails()
                {
                    Detail = problemDescription.Detail,
                    Instance = problemDescription.Instance,
                    Status = (int)responseMessage.StatusCode,
                    Title = problemDescription.Title,
                    Type = problemDescription.Type,
                };
                foreach (var kvp in problemDescription.Extensions)
                {
                    problemDetailsCopy.Extensions.Add(kvp);
                }
                return HypermediaResult.Error<Unit>(HypermediaProblem.ProblemDetails(problemDetailsCopy));
            }

            return HypermediaResult.Error<Unit>(HypermediaProblem.StatusCode((int)responseMessage.StatusCode));
        }

        private async Task<(bool hasProblemDescription, ProblemDetails? problemDescription)> TryReadProblemStringAsync(HttpResponseMessage response)
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

        protected override async Task<HypermediaResult<Stream>> ResponseAsStreamAsync(HttpResponseMessage responseMessage)
        {
            try
            {
                return await responseMessage.Content.ReadAsStreamAsync();
            }
            catch (Exception e)
            {
                return HypermediaResult<Stream>.Error(HypermediaProblem.Exception(e));
            }
        }

        protected override HypermediaResult<Uri> GetLocation(HttpResponseMessage responseMessage)
        {
            var location = responseMessage.Headers.Location;
            if (location is null)
            {
                return HypermediaResult.Error<Uri>(HypermediaProblem.InvalidResponse("hypermedia function did not return a result resource location."));
            }
            return HypermediaResult.Ok(location);
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