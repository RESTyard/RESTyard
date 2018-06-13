namespace Hypermedia.Client.Resolver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    using global::Hypermedia.Client.Authentication;
    using global::Hypermedia.Client.Exceptions;
    using global::Hypermedia.Client.Hypermedia;
    using global::Hypermedia.Client.Hypermedia.Commands;
    using global::Hypermedia.Client.ParameterSerializer;
    using global::Hypermedia.Client.Reader;
    using global::Hypermedia.Client.Reader.ProblemJson;
    using global::Hypermedia.MediaTypes;

    public class HttpHypermediaResolver : IHypermediaResolver, IDisposable
    {
        private readonly IParameterSerializer parameterSerializer;
        private IHypermediaReader hypermediaReader;
        private HttpClient httpClient;

        private UsernamePasswordCredentials UsernamePasswordCredentials { get; set; }

        public HttpHypermediaResolver(IParameterSerializer parameterSerializer)
        {
            // todo maybe pass HttpClient as dependency so it can be modified by the user
            this.parameterSerializer = parameterSerializer;
            this.InitializeHttpClient();
        }

        public void InitializeHypermediaReader(IHypermediaReader reader)
        {
            this.hypermediaReader = reader;
        }

        public async Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve) where T : HypermediaClientObject
        {
            var result = await this.httpClient.GetAsync(uriToResolve);
            if (!result.IsSuccessStatusCode)
            {
                ThrowRequestErrorException(result);
            }

            var hypermediaObjectSiren = await result.Content.ReadAsStringAsync(); //TODO READ AS STREAM for pref

            if (this.hypermediaReader == null)
            {
                throw new Exception($"Please setup the hypermediaReader before using the resolver. see {nameof(this.InitializeHypermediaReader)}");
            }

            if (!(this.hypermediaReader.Read(hypermediaObjectSiren) is T desiredResultObject))
            {
                throw new Exception($"Could not retrieve result as {typeof(T).Name} ");
            }

            var resolverResult = new ResolverResult<T>();
            resolverResult.ResultObject = desiredResultObject;
            resolverResult.Success = true;

            return resolverResult;
        }

        private static void ThrowRequestErrorException(HttpResponseMessage result)
        {
            var hasProblemDescription = ProblemJsonReader.TryReadProblemJson(result, out var problemDescription);
            if (hasProblemDescription)
            {
                throw new HypermediaProblemException(problemDescription);
            }

            ThrowExceptionFromResult(result);
        }

        public async Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method)
        {
            var responseMessage = await this.SendCommand(uri, method);
            var actionResult = HandleResponse(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject)
        {
            var serializedParameters = this.ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await this.SendCommand(uri, method, serializedParameters);
            var actionResult = HandleResponse(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method) where T : HypermediaClientObject
        {
            var responseMessage = await this.SendCommand(uri, method);
            var actionResult = this.HandleFunctionResponse<T>(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject) where T : HypermediaClientObject
        {
            var serializedParameters = this.ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await this.SendCommand(uri, method, serializedParameters);
            var actionResult = this.HandleFunctionResponse<T>(responseMessage);
            return actionResult;
        }

        private static void ThrowExceptionFromResult(HttpResponseMessage result)
        {
            var detail = result.Content?.ReadAsStringAsync().Result;
            throw new HypermediaClientException($"{result.ReasonPhrase} ({result.StatusCode})", $"{detail}");
        }

        private string ProcessParameters(List<ParameterDescription> parameterDescriptions, object parameterObject)
        {
            if (parameterObject == null)
            {
                throw new Exception("Parameter is described but not passed by action.");
            }

            var parameterDescription = GetParameterDescription(parameterDescriptions);

            var serializedParameters = this.parameterSerializer.SerializeParameterObject(parameterDescription.Name, parameterObject);
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

        private static HypermediaCommandResult HandleResponse(HttpResponseMessage responseMessage)
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                ThrowRequestErrorException(responseMessage);
            }

            var actionResult = new HypermediaCommandResult();
            actionResult.Success = true;
            return actionResult;
        }

        private HypermediaFunctionResult<T> HandleFunctionResponse<T>(HttpResponseMessage responseMessage) where T : HypermediaClientObject
        {
            if (!responseMessage.IsSuccessStatusCode)
            {
                ThrowRequestErrorException(responseMessage);
            }

            var actionResult = new HypermediaFunctionResult<T>();
            actionResult.Success = true;

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
            var httpMethod = this.GetHttpMethod(method);
            var request = new HttpRequestMessage(httpMethod, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, DefaultMediaTypes.ApplicationJson);//CONTENT-TYPE header    
            }

            var responseMessage = await this.httpClient.SendAsync(request);
            
            return responseMessage;
        }

        private void InitializeHttpClient()
        {
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Accept.Clear();
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(DefaultMediaTypes.Siren));

            if (this.HasCredentials())
            {
                this.httpClient.DefaultRequestHeaders.Authorization = CreateBasicAuthHeaderValue(this.UsernamePasswordCredentials);
            }
        }

        private static AuthenticationHeaderValue CreateBasicAuthHeaderValue(UsernamePasswordCredentials credentials)
        {
            var encodedCredentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials.User + ":" + credentials.Password));
            return new AuthenticationHeaderValue("Basic", encodedCredentials);
        }

        private bool HasCredentials()
        {
            return this.UsernamePasswordCredentials != null;
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
            this.InitializeHttpClient();
            // todo if using a cache clear it, new user migth not be able to access cached content
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }
    }
}