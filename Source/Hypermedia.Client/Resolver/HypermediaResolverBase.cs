using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver.Caching;

namespace RESTyard.Client.Resolver
{
    public abstract class HypermediaResolverBase<
            TNetworkResponseMessage,
            TLinkHcoCacheEntry,
            TLinkHcoCacheEntryConfiguration>
        : IHypermediaResolver
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
        where TLinkHcoCacheEntryConfiguration : LinkHcoCacheEntryConfiguration
    {
        private bool alreadyDisposed;

        protected HypermediaResolverBase(
            IHypermediaReader hypermediaReader,
            IParameterSerializer parameterSerializer,
            IProblemStringReader problemReader,
            ILinkHcoCache<TLinkHcoCacheEntry> linkHcoCache)
        {
            HypermediaReader = hypermediaReader;
            ParameterSerializer = parameterSerializer;
            ProblemReader = problemReader;
            LinkHcoCache = linkHcoCache;
        }

        protected IHypermediaReader HypermediaReader { get; }

        protected IParameterSerializer ParameterSerializer { get; }

        protected IProblemStringReader ProblemReader { get; }

        protected ILinkHcoCache<TLinkHcoCacheEntry> LinkHcoCache { get; }

        public async Task<ResolverResult<T>> ResolveLinkAsync<T>(
            Uri uriToResolve,
            bool forceResolve = false)
            where T : HypermediaClientObject
        {
            TNetworkResponseMessage response;
            if (this.LinkHcoCache.TryGetValue(uriToResolve, out var cacheEntry))
            {
                var verificationResult = await this.VerifyIfCacheEntryCanBeUsedAsync(uriToResolve, cacheEntry, DateTimeOffset.Now, forceResolve);
                var cacheEntryCanBeUsed = verificationResult.Match(
                    canBeUsed => true,
                    canNotBeUsed => false,
                    useThisResponseInstead => false);
                if (cacheEntryCanBeUsed)
                {
                    var hco = (T)this.HypermediaReader.Read(cacheEntry.LinkResponseContent, this);
                    return new ResolverResult<T>(
                        success: true,
                        hco);
                }
                else
                {
                    this.LinkHcoCache.Remove(uriToResolve);
                }

                response = await verificationResult.Match(
                    canBeUsed => this.ResolveAsync(uriToResolve),
                    canNotBeUsed => this.ResolveAsync(uriToResolve),
                    useResponse => Task.FromResult(useResponse.Response));
            }
            else
            {
                response = await this.ResolveAsync(uriToResolve);
            }

            var cacheConfiguration = this.GetCacheConfigurationFromResponse(response, DateTimeOffset.Now);
            bool serializeToString = cacheConfiguration.ShouldBeAddedToCache();
            var (resolverResult, hcoAsString) = await this.HandleLinkResponseAsync<T>(response, serializeToString);

            if (resolverResult.Success
                && cacheConfiguration.ShouldBeAddedToCache()
                && !string.IsNullOrEmpty(hcoAsString))
            {
                var entry = GetCacheEntryFromConfiguration(hcoAsString, cacheConfiguration);
                this.LinkHcoCache.Set(uriToResolve, entry);
            }

            return resolverResult;
        }

        protected abstract Task<CacheEntryVerificationResult<TNetworkResponseMessage>> VerifyIfCacheEntryCanBeUsedAsync(
            Uri uriToResolve,
            TLinkHcoCacheEntry cacheEntry,
            DateTimeOffset assumedNow,
            bool forceResolve);

        protected abstract TLinkHcoCacheEntryConfiguration GetCacheConfigurationFromResponse(
            TNetworkResponseMessage response,
            DateTimeOffset assumedNow);

        protected abstract TLinkHcoCacheEntry GetCacheEntryFromConfiguration(
            string linkResponseContent,
            TLinkHcoCacheEntryConfiguration cacheConfiguration);

        public async Task<HypermediaCommandResult> ResolveActionAsync(
            Uri uri,
            string method)
        {
            var responseMessage = await this.SendCommandAsync(uri, method);
            var actionResult = await this.HandleActionResponseAsync(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaCommandResult> ResolveActionAsync(
            Uri uri,
            string method,
            List<ParameterDescription> parameterDescriptions,
            object parameterObject)
        {
            var serializedParameters = this.ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await this.SendCommandAsync(uri, method, serializedParameters);
            var actionResult = await HandleActionResponseAsync(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(
            Uri uri,
            string method) where T : HypermediaClientObject
        {
            var responseMessage = await SendCommandAsync(uri, method);
            var functionResult = await this.HandleFunctionResponseAsync<T>(responseMessage);
            return functionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(
            Uri uri,
            string method,
            List<ParameterDescription> parameterDescriptions,
            object parameterObject) where T : HypermediaClientObject
        {
            var serializedParameters = this.ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await this.SendCommandAsync(uri, method, serializedParameters);
            var actionResult = await this.HandleFunctionResponseAsync<T>(responseMessage);
            return actionResult;
        }

        protected async Task<(ResolverResult<T>, string)> HandleLinkResponseAsync<T>(
            TNetworkResponseMessage responseMessage,
            bool serializeToString)
            where T : HypermediaClientObject
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var hypermediaObjectSirenStream = await this.ResponseAsStreamAsync(responseMessage);

            HypermediaClientObject hypermediaClientObject;
            string serialized = string.Empty;
            if (serializeToString)
            {
                (hypermediaClientObject, serialized) =
                    await this.HypermediaReader.ReadAndSerializeAsync(hypermediaObjectSirenStream, this);
            }
            else
            {
                hypermediaClientObject = await this.HypermediaReader.ReadAsync(hypermediaObjectSirenStream, this);
            }
            if (!(hypermediaClientObject is T desiredResultObject))
            {
                throw new Exception($"Could not retrieve result as {typeof(T).Name}.");
            }

            var resolverResult = new ResolverResult<T>(
                success: true,
                desiredResultObject);
            return (resolverResult, serialized);
        }

        protected async Task<HypermediaCommandResult> HandleActionResponseAsync(TNetworkResponseMessage responseMessage)
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var actionResult = new HypermediaCommandResult()
            {
                Success = true,
            };
            return actionResult;
        }

        protected async Task<HypermediaFunctionResult<T>> HandleFunctionResponseAsync<T>(
            TNetworkResponseMessage responseMessage)
            where T : HypermediaClientObject
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var location = this.GetLocation(responseMessage);
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
                    Resolver = this,
                },
            };

            return actionResult;
        }

        protected string ProcessParameters(IList<ParameterDescription> parameterDescriptions, object parameterObject)
        {
            if (parameterObject is null)
            {
                throw new Exception("Parameter is described but not passed by action.");
            }

            var parameterDescription = GetParameterDescription(parameterDescriptions);

            var serializedParameters =
                this.ParameterSerializer.SerializeParameterObject(parameterDescription.Name, parameterObject);
            return serializedParameters;
        }

        protected static ParameterDescription GetParameterDescription(IList<ParameterDescription> parameterDescriptions)
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

        protected abstract Task<TNetworkResponseMessage> ResolveAsync(Uri uriToResolve);

        protected abstract Task<TNetworkResponseMessage> SendCommandAsync(
            Uri uri,
            string method,
            string payload = null);

        protected abstract Task EnsureRequestIsSuccessfulAsync(TNetworkResponseMessage responseMessage);

        protected abstract Task<Stream> ResponseAsStreamAsync(TNetworkResponseMessage responseMessage);

        protected abstract Uri GetLocation(TNetworkResponseMessage responseMessage);

        ~HypermediaResolverBase()
        {
            this.Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.alreadyDisposed)
            {
                return;
            }

            this.alreadyDisposed = true;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}