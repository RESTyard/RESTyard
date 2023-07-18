using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FunicularSwitch;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver.Caching;
using RESTyard.MediaTypes;

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

        public async Task<HypermediaResult<T>> ResolveLinkAsync<T>(
            Uri uriToResolve,
            bool forceResolve = false)
            where T : HypermediaClientObject
        {
            HypermediaResult<TNetworkResponseMessage> networkResult;
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
                    return HypermediaResult.Ok(hco);
                }
                else
                {
                    this.LinkHcoCache.Remove(uriToResolve);
                }

                networkResult = await verificationResult.Match(
                    canBeUsed => this.ResolveAsync(uriToResolve),
                    canNotBeUsed => this.ResolveAsync(uriToResolve),
                    async useResponse => HypermediaResult.Ok(useResponse.Response));
            }
            else
            {
                networkResult = await this.ResolveAsync(uriToResolve);
            }

            return await networkResult
                .Bind(async response =>
                {
                    var cacheConfiguration = this.GetCacheConfigurationFromResponse(response, DateTimeOffset.Now);
                    bool serializeToString = cacheConfiguration.ShouldBeAddedToCache();
                    var linkResult = await this.HandleLinkResponseAsync<T>(response, serializeToString);

                    linkResult.Match(ok =>
                    {
                        var hcoAsString = ok.HcoAsString;
                        if (cacheConfiguration.ShouldBeAddedToCache()
                            && !string.IsNullOrEmpty(hcoAsString))
                        {
                            var entry = GetCacheEntryFromConfiguration(hcoAsString, cacheConfiguration);
                            this.LinkHcoCache.Set(uriToResolve, entry);
                        }
                    });

                    return linkResult.Bind<T>(ok => ok.ResultHco);
                });
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

        public async Task<HypermediaResult<Unit>> ResolveActionAsync(
            Uri uri,
            string method)
        {
            return await this.SendCommandAsync(uri, method)
                .Bind(responseMessage => this.HandleActionResponseAsync(responseMessage));
        }

        public async Task<HypermediaResult<Unit>> ResolveActionAsync(
            Uri uri,
            string method,
            List<ParameterDescription> parameterDescriptions,
            object parameterObject)
        {
            return await this.ProcessParameters(parameterDescriptions, parameterObject)
                .Bind(serializedParameters => this.SendCommandAsync(uri, method, serializedParameters))
                .Bind(responseMessage => this.HandleActionResponseAsync(responseMessage));
        }

        public async Task<HypermediaResult<MandatoryHypermediaLink<T>>> ResolveFunctionAsync<T>(
            Uri uri,
            string method) where T : HypermediaClientObject
        {
            return await SendCommandAsync(uri, method)
                .Bind(responseMessage => this.HandleFunctionResponseAsync<T>(responseMessage));
        }

        public async Task<HypermediaResult<MandatoryHypermediaLink<T>>> ResolveFunctionAsync<T>(
            Uri uri,
            string method,
            List<ParameterDescription> parameterDescriptions,
            object parameterObject) where T : HypermediaClientObject
        {
            return await this.ProcessParameters(parameterDescriptions, parameterObject)
                .Bind(serializedParameters => this.SendCommandAsync(uri, method, serializedParameters))
                .Bind(responseMessage => this.HandleFunctionResponseAsync<T>(responseMessage));
        }

        protected async Task<HypermediaResult<(T ResultHco, string HcoAsString)>> HandleLinkResponseAsync<T>(
            TNetworkResponseMessage responseMessage,
            bool serializeToString)
            where T : HypermediaClientObject
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage)
                .Bind(_ => this.ResponseAsStreamAsync(responseMessage))
                .Bind(async hypermediaObjectSirenStream =>
                {
                    HypermediaClientObject hypermediaClientObject;
                    string serialized = string.Empty;
                    if (serializeToString)
                    {
                        (hypermediaClientObject, serialized) =
                            await this.HypermediaReader.ReadAndSerializeAsync(hypermediaObjectSirenStream, this);
                    }
                    else
                    {
                        hypermediaClientObject =
                            await this.HypermediaReader.ReadAsync(hypermediaObjectSirenStream, this);
                    }

                    if (!(hypermediaClientObject is T desiredResultObject))
                    {
                        return HypermediaResult.Error<(T, string)>(HypermediaProblem.InvalidResponse($"Could not retrieve result as {typeof(T).Name}."));
                    }

                    return HypermediaResult.Ok((desiredResultObject, serialized));
                });
        }

        protected async Task<HypermediaResult<Unit>> HandleActionResponseAsync(TNetworkResponseMessage responseMessage)
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage);
        }

        protected async Task<HypermediaResult<MandatoryHypermediaLink<T>>> HandleFunctionResponseAsync<T>(
            TNetworkResponseMessage responseMessage)
            where T : HypermediaClientObject
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage)
                .Bind(_ => this.GetLocation(responseMessage))
                .Bind(location =>
                {
                    var actionResult = HypermediaResult.Ok(new MandatoryHypermediaLink<T>()
                    {
                        Uri = location,
                        Resolver = this,
                    });

                    return actionResult;
                });
        }

        protected HypermediaResult<string> ProcessParameters(IList<ParameterDescription> parameterDescriptions, object? parameterObject)
        {
            if (parameterObject is null)
            {
                return HypermediaResult.Error<string>(
                    HypermediaProblem.InvalidRequest("Parameter is described but not passed by action."));
            }

            return GetParameterDescription(parameterDescriptions)
                .Bind(parameterDescription => {
                    var serializedParameters =
                        this.ParameterSerializer.SerializeParameterObject(parameterDescription.Name, parameterObject);
                    return HypermediaResult.Ok(serializedParameters);
                });
        }

        protected static HypermediaResult<ParameterDescription> GetParameterDescription(IList<ParameterDescription> parameterDescriptions)
        {
            if (parameterDescriptions.Count == 0)
            {
                return HypermediaResult<ParameterDescription>.Error(HypermediaProblem.InvalidRequest("Parameter not described."));
            }

            // todo allow more fields
            if (parameterDescriptions.Count > 1)
            {
                return HypermediaResult<ParameterDescription>.Error(HypermediaProblem.InvalidRequest("Only one action parameter is supported."));
            }

            // todo allow more types
            var parameterDescription = parameterDescriptions.First();
            if (!parameterDescription.Type.Equals(DefaultMediaTypes.ApplicationJson))
            {
                return HypermediaResult<ParameterDescription>.Error(HypermediaProblem.InvalidRequest("Only one action type 'application/json' is supported."));
            }
            return HypermediaResult.Ok(parameterDescription);
        }

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> ResolveAsync(Uri uriToResolve);

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> SendCommandAsync(
            Uri uri,
            string method,
            string payload = null);

        protected abstract Task<HypermediaResult<Unit>> EnsureRequestIsSuccessfulAsync(TNetworkResponseMessage responseMessage);

        protected abstract Task<HypermediaResult<Stream>> ResponseAsStreamAsync(TNetworkResponseMessage responseMessage);

        protected abstract HypermediaResult<Uri> GetLocation(TNetworkResponseMessage responseMessage);

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