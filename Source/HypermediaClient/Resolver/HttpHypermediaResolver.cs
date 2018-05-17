using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hypermedia.Util;
using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Commands;
using HypermediaClient.ParameterSerializer;

namespace HypermediaClient.Resolver
{
    using HypermediaClient.Authentication;

    public  class HttpHypermediaResolver : IHypermediaResolver
    {
        private readonly IParameterSerializer parameterSerializer;
        private IHypermediaReader hypermediaReader;

        private UsernamePasswordCredentials UsernamePasswordCredentials { get; set; }

        public HttpHypermediaResolver(IParameterSerializer parameterSerializer)
        {
            this.parameterSerializer = parameterSerializer;
        }

        public void InitializeHypermediaReader(IHypermediaReader reader)
        {
            this.hypermediaReader = reader;
        }

        public async Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve) where T : HypermediaClientObject
        {
            var httpClient = CreateHttpClient();

            var result = await httpClient.GetAsync(uriToResolve);
            var resolverResult =  new ResolverResult<T>();

            if (!result.IsSuccessStatusCode)
            {
                return resolverResult;
            }

            var hypermediaObjectSiren = await result.Content.ReadAsStringAsync(); //TODO READ AS STREAM for pref

            if (hypermediaReader == null)
            {
                throw new Exception($"Please setup the hypermediaReader before using the resolver. see {nameof(InitializeHypermediaReader)}");
            }

            if (!(this.hypermediaReader.Read(hypermediaObjectSiren) is T desiredResultObject))
            {
                throw new Exception($"Could not retrieve result as {typeof(T).Name} ");
            }
            resolverResult.ResultObject = desiredResultObject;
            resolverResult.Success = true;

            return resolverResult;
        }

        public async Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method)
        {
            var responseMessage = await SendCommand(uri, method);
            var actionResult = HandleResponse(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject)
        {
            var serializedParameters = ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await SendCommand(uri, method, serializedParameters);
            var actionResult = HandleResponse(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method) where T : HypermediaClientObject
        {
            var responseMessage = await SendCommand(uri, method);
            var actionResult = HandleFunctionResponse<T>(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject) where T : HypermediaClientObject
        {
            var serializedParameters = ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await SendCommand(uri, method, serializedParameters);
            var actionResult = HandleFunctionResponse<T>(responseMessage);
            return actionResult;
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
            if (!parameterDescription.Type.Equals(DefaultContentTypes.ApplicationJson))
            {
                throw new Exception("Only one action type 'application/json' is supported.");
            }
            return parameterDescription;
        }

        private static HypermediaCommandResult HandleResponse(HttpResponseMessage responseMessage)
        {
            var actionResult = new HypermediaCommandResult();
            if (responseMessage.IsSuccessStatusCode)
            {
                actionResult.Success = true;
            }
            return actionResult;
        }

        private HypermediaFunctionResult<T> HandleFunctionResponse<T>(HttpResponseMessage responseMessage) where T : HypermediaClientObject
        {
            var actionResult = new HypermediaFunctionResult<T>();
            if (responseMessage.IsSuccessStatusCode)
            {
                actionResult.Success = true;
            }

            var location = responseMessage.Headers.Location;
            if (location == null)
            {
                throw new Exception("hypermedia function did not return a result resource location.");
            }

            actionResult.ResultLocation.Uri = location;
            actionResult.ResultLocation.Resolver = this;

            return actionResult;
        }

        private async Task<HttpResponseMessage> SendCommand(Uri uri, string method, string payload = null)
        {
            var httpClient = CreateHttpClient();
            var httpMethod = GetHttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, DefaultContentTypes.ApplicationJson);//CONTENT-TYPE header    
            }

            var responseMessage = await httpClient.SendAsync(request);
            
            return responseMessage;
        }

        private HttpClient CreateHttpClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(DefaultContentTypes.Siren));

            if (HasCredentials())
            {
                httpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeaderValue(this.UsernamePasswordCredentials);
            }

            return httpClient;
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

        private HttpMethod GetHttpMethod(string method)
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
            this.UsernamePasswordCredentials = usernamePasswordCredentials;

            // todo if using a cache clear it, new user migth not be able to access
        }
    }
}