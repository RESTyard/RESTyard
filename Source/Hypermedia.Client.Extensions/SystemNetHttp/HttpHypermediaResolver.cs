using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Authentication;
using Bluehands.Hypermedia.Client.Exceptions;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver;
using Bluehands.Hypermedia.Client.Resolver.Caching;
using Bluehands.Hypermedia.MediaTypes;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public class HttpHypermediaResolver : IHypermediaResolver, IHttpHypermediaResolverConfiguration, IDisposable
    {
        private readonly IParameterSerializer parameterSerializer;
        private readonly IProblemStringReader problemReader;
        private readonly ILinkHcoCache<HttpLinkHcoCacheEntry> linkHcoCache;
        private IHypermediaReader hypermediaReader;
        private HttpClient httpClient;

        private UsernamePasswordCredentials UsernamePasswordCredentials { get; set; }

        private Action<HttpRequestHeaders> AddCustomDefaultHeadersAction { get; set; }

        public HttpHypermediaResolver(
            IParameterSerializer parameterSerializer,
            IProblemStringReader problemReader,
            ILinkHcoCache<HttpLinkHcoCacheEntry> linkHcoCache)
        {
            // todo maybe pass HttpClient as dependency so it can be modified by the user
            this.parameterSerializer = parameterSerializer;
            this.problemReader = problemReader;
            this.linkHcoCache = linkHcoCache;
            InitializeHttpClient();
        }

        public void InitializeHypermediaReader(IHypermediaReader reader)
        {
            hypermediaReader = reader;
        }

        public async Task<ResolverResult<T>> ResolveLinkAsync<T>(
            Uri uriToResolve,
            bool forceResolve = false)
            where T : HypermediaClientObject
        {
            HttpResponseMessage response;
            bool forceRevalidate = forceResolve;
            if (this.linkHcoCache.TryGetValue(uriToResolve, out var cacheEntry))
            {
                var mustRevalidate = forceRevalidate || cacheEntry.IsRevalidationRequired(DateTimeOffset.Now);
                if (!mustRevalidate)
                {
                    var hco = this.hypermediaReader.Read(cacheEntry.LinkResponseContent);
                    return new ResolverResult<T>()
                    {
                        Success = true,
                        ResultObject = (T)hco,
                    };
                }
                var request = CreateRevalidationRequest(uriToResolve, cacheEntry);
                response = await this.httpClient.SendAsync(request, CancellationToken.None);
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    this.UpdateCacheEntry(uriToResolve, response, cacheEntry);

                    var hco = this.hypermediaReader.Read(cacheEntry.LinkResponseContent);
                    return new ResolverResult<T>()
                    {
                        Success = true,
                        ResultObject = (T)hco,
                    };
                }
                else
                {
                    this.linkHcoCache.Remove(uriToResolve);
                }
            }
            else
            {
                response = await httpClient.GetAsync(uriToResolve);
            }

            var cacheConfiguration = CacheEntryConfiguration.FromHttpResponse(response, DateTimeOffset.Now);
            bool exportToString = cacheConfiguration.ShouldCache();
            var (resolverResult, hcoAsString) = await HandleLinkResponseAsync<T>(response, exportToString);

            if (resolverResult.Success && exportToString && !string.IsNullOrEmpty(hcoAsString))
            {
                var entry = HttpLinkHcoCacheEntry.FromConfiguration(hcoAsString, cacheConfiguration);
                this.linkHcoCache.Set(uriToResolve, entry);
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
                this.linkHcoCache.Remove(uriToResolve);
            }
            else if (!newConfiguration.ShouldCache())
            {
                this.linkHcoCache.Remove(uriToResolve);
            }
            else if (HasUpdatedCacheConfiguration(oldEntry, newConfiguration))
            {
                this.linkHcoCache.Replace(
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

        public async Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method)
        {
            var responseMessage = await SendCommandAsync(uri, method);
            var actionResult = await HandleActionResponseAsync(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject)
        {
            var serializedParameters = ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await SendCommandAsync(uri, method, serializedParameters);
            var actionResult = await HandleActionResponseAsync(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method) where T : HypermediaClientObject
        {
            var responseMessage = await SendCommandAsync(uri, method);
            var actionResult = await HandleFunctionResponseAsync<T>(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject) where T : HypermediaClientObject
        {
            var serializedParameters = ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await SendCommandAsync(uri, method, serializedParameters);
            var actionResult = await HandleFunctionResponseAsync<T>(responseMessage);
            return actionResult;
        }

        private async Task EnsureRequestIsSuccessfulAsync(HttpResponseMessage result)
        {
            if (result.IsSuccessStatusCode)
            {
                return;
            }

            var innerException = GetInnerException(result);

            var (hasProblemDescription, problemDescription) = await this.TryReadProblemStringAsync(result);
            if (hasProblemDescription)
            {
                throw new HypermediaProblemException(problemDescription, innerException);
            }

            var message = innerException.Message ?? string.Empty;
            throw new RequestNotSuccessfulException(message, (int)result.StatusCode, innerException);
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

        private async Task<(bool hasProblemDescription, ProblemDescription problemDescription)> TryReadProblemStringAsync(HttpResponseMessage response)
        {
            if (response.Content == null)
            {
                return (false, null);
            }
            try
            {
                var contentAsString = await response.Content.ReadAsStringAsync();
                var result = this.problemReader.TryReadProblemString(contentAsString, out var problemDescription);
                return (result, problemDescription);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }

        private string ProcessParameters(List<ParameterDescription> parameterDescriptions, object parameterObject)
        {
            if (parameterObject == null)
            {
                throw new Exception("Parameter is described but not passed by action.");
            }

            var parameterDescription = GetParameterDescription(parameterDescriptions);

            var serializedParameters = parameterSerializer.SerializeParameterObject(parameterDescription.Name, parameterObject);
            return serializedParameters;
        }

        private static ParameterDescription GetParameterDescription(List<ParameterDescription> parameterDescriptions)
        {
            if (parameterDescriptions.Count == 0)
            {
                throw new Exception("Parameter not described.");
            }

            // todo allow more fields
            if (parameterDescriptions.Count > 1)
            {
                throw new Exception("Only one action parameter is supported.");
            }

            // todo allow more types
            var parameterDescription = parameterDescriptions.First();
            if (!parameterDescription.Type.Equals(DefaultMediaTypes.ApplicationJson))
            {
                throw new Exception("Only one action type 'application/json' is supported.");
            }
            return parameterDescription;
        }

        private async Task<(ResolverResult<T>, string)> HandleLinkResponseAsync<T>(HttpResponseMessage responseMessage, bool exportToString)
            where T : HypermediaClientObject
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var hypermediaObjectSiren = await responseMessage.Content.ReadAsStreamAsync();

            if (this.hypermediaReader == null)
            {
                throw new Exception(
                    $"Please setup the hypermediaReader before using the resolver. see {nameof(InitializeHypermediaReader)}");
            }

            HypermediaClientObject hypermediaClientObject;
            string export = string.Empty;
            if (exportToString)
            {
                (hypermediaClientObject, export) = await this.hypermediaReader.ReadAndExportAsync(hypermediaObjectSiren);
            }
            else
            {
                hypermediaClientObject = await this.hypermediaReader.ReadAsync(hypermediaObjectSiren);
            }

            if (!(hypermediaClientObject is T desiredResultObject))
            {
                throw new Exception($"Could not retrieve result as {typeof(T).Name} ");
            }

            var resolverResult = new ResolverResult<T>()
            {
                ResultObject = desiredResultObject,
                Success = true,
            };
            return (resolverResult, export);
        }

        private async Task<HypermediaCommandResult> HandleActionResponseAsync(HttpResponseMessage responseMessage)
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var actionResult = new HypermediaCommandResult()
            {
                Success = true
            };
            return actionResult;
        }

        private async Task<HypermediaFunctionResult<T>> HandleFunctionResponseAsync<T>(HttpResponseMessage responseMessage) where T : HypermediaClientObject
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var location = responseMessage.Headers.Location;
            if (location == null)
            {
                throw new Exception("hypermedia function did not return a result resource location.");
            }

            var actionResult = new HypermediaFunctionResult<T>
            {
                Success = true,
                ResultLocation =
                {
                    Uri = location,
                    Resolver = this
                },
            };

            return actionResult;
        }

        private async Task<HttpResponseMessage> SendCommandAsync(Uri uri, string method, string payload = null)
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

        private void InitializeHttpClient()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }

            httpClient.DefaultRequestHeaders.Clear();

            if (HasCustomHeaders())
            {
                AddCustomDefaultHeadersAction.Invoke(httpClient.DefaultRequestHeaders);
            }

            if (HasCredentials())
            {
                httpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeaderValue(UsernamePasswordCredentials);
            }

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(DefaultMediaTypes.Siren));
        }

        private bool HasCustomHeaders()
        {
            return AddCustomDefaultHeadersAction != null;
        }

        private static AuthenticationHeaderValue CreateBasicAuthHeaderValue(UsernamePasswordCredentials credentials)
        {
            var encodedCredentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials.User + ":" + credentials.Password));
            return new AuthenticationHeaderValue("Basic", encodedCredentials);
        }

        private bool HasCredentials()
        {
            return UsernamePasswordCredentials != null;
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            switch (method)
            {
                case "POST":
                    return HttpMethod.Post;
                case "GET":
                    return HttpMethod.Get;
                case "DELETE":
                    return HttpMethod.Delete;
                case "PUT":
                    return HttpMethod.Put;
                default:
                    throw new Exception($"Unknown method: '{method}'");
            }
        }

        public void SetCredentials(UsernamePasswordCredentials usernamePasswordCredentials)
        {
            UsernamePasswordCredentials = usernamePasswordCredentials;
            InitializeHttpClient();
            ClearCache();
        }

        private void ClearCache()
        {
            this.linkHcoCache?.Clear();
        }

        public void SetCustomDefaultHeaders(Action<HttpRequestHeaders> addCustomDefaultHeadersAction)
        {
            AddCustomDefaultHeadersAction = addCustomDefaultHeadersAction;
            InitializeHttpClient();
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}