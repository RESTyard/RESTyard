namespace Hypermedia.Client.Resolver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Bluehands.Hypermedia.MediaTypes;

    using Authentication;
    using Exceptions;
    using Hypermedia;
    using Hypermedia.Commands;
    using ParameterSerializer;
    using Reader;
    using Reader.ProblemJson;
    

    public class HttpHypermediaResolver : IHypermediaResolver, IDisposable
    {
        private readonly IParameterSerializer parameterSerializer;
        private IHypermediaReader hypermediaReader;
        private HttpClient httpClient;

        private UsernamePasswordCredentials UsernamePasswordCredentials { get; set; }

        private Action<HttpRequestHeaders> AddCustomDefaultHeadersAction { get; set; }

        public HttpHypermediaResolver(IParameterSerializer parameterSerializer)
        {
            // todo maybe pass HttpClient as dependency so it can be modified by the user
            this.parameterSerializer = parameterSerializer;
            InitializeHttpClient();
        }

        public void InitializeHypermediaReader(IHypermediaReader reader)
        {
            hypermediaReader = reader;
        }

        public async Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve) where T : HypermediaClientObject
        {
            var result = await httpClient.GetAsync(uriToResolve);
            EnsureRequestIsSuccessful(result);

            var hypermediaObjectSiren = await result.Content.ReadAsStringAsync(); //TODO READ AS STREAM for pref

            if (hypermediaReader == null)
            {
                throw new Exception($"Please setup the hypermediaReader before using the resolver. see {nameof(InitializeHypermediaReader)}");
            }

            if (!(hypermediaReader.Read(hypermediaObjectSiren) is T desiredResultObject))
            {
                throw new Exception($"Could not retrieve result as {typeof(T).Name} ");
            }

            var resolverResult = new ResolverResult<T>();
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

        private static void EnsureRequestIsSuccessful(HttpResponseMessage result)
        {
            if (result.IsSuccessStatusCode)
            {
                return;
            }

            var innerException = GetInnerException(result);

            var hasProblemDescription = ProblemJsonReader.TryReadProblemJson(result, out var problemDescription);
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

        private static HypermediaCommandResult HandleResponse(HttpResponseMessage responseMessage)
        {
            EnsureRequestIsSuccessful(responseMessage);

            var actionResult = new HypermediaCommandResult();
            actionResult.Success = true;
            return actionResult;
        }

        private HypermediaFunctionResult<T> HandleFunctionResponse<T>(HttpResponseMessage responseMessage) where T : HypermediaClientObject
        {
            EnsureRequestIsSuccessful(responseMessage);

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
            UsernamePasswordCredentials = usernamePasswordCredentials;
            InitializeHttpClient();
            // todo if using a cache clear it, new user migth not be able to access cached content
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